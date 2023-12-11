// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Koenigz.PerfectCulling.IO;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Koenigz.PerfectCulling
{
    [PreferBinarySerialization]
    public class PerfectCullingVolumeBakeData : PerfectCullingBakeData
    {
        [System.Serializable]
        public struct VisibilitySet
        {
            public byte[] compressed;
            public ushort len;
        }

        [System.Serializable]
        public struct RawData
        {
	        public ushort[] uncompressed;
        }

        public Vector3 cellCount;
        public Vector3 cellSize;

        public Quaternion orientation;
        
        public VisibilitySet[] data;
        public RawData[] rawData;

        public int maxStoredIndex = -1;
        public int numberOfGroups = -1;

        public void SetVolumeBakeData(PerfectCullingVolume volume)
        {
	        cellSize = volume.bakeCellSize;
	        cellCount = new Vector3(volume.volumeSize.x / cellSize.x, volume.volumeSize.y / cellSize.y, volume.volumeSize.z / cellSize.z);
        }
        
        public override void PrepareForBake(PerfectCullingBakingBehaviour bakingBehaviour)
        {
            orientation = bakingBehaviour.transform.rotation;

            data = new VisibilitySet[bakingBehaviour.GetSamplingPositions().Count];
            rawData = new RawData[data.Length];

            maxStoredIndex = -1;
            numberOfGroups = bakingBehaviour.bakeGroups.Length;
        }

        private const int MaxValue = 15;
        
        private const int HeaderBitSize = 2;
        static uint[] BITS = new uint[4]
        {
	        1,
	        2,
	        3,
	        4
        };

        public override void SetRawData(int index, ushort[] indices, bool validateData = true)
        {
	        if (validateData && indices.Length <= 0)
	        {
		        Debug.LogWarning("Cell without any visible renderers. Should be highly unlikely to happen unless you are performing a multi-scene bake with additional occluders.");
	        }

	        rawData[index] = new RawData()
	        {
		        uncompressed = indices
	        };

	        foreach (ushort i in indices)
	        {
		        maxStoredIndex = Mathf.Max(i, maxStoredIndex);
	        }
        }

        void SetDataCompressed(int index, ushort[] indices, BitStreamReader streamReader, BitStreamWriter streamWriter)
        {
	        if (indices.Length <= 0)
	        {
		        return;
	        }
	        
	        void AddValue(int value, List<int> values)
	        {
		        while (value >= MaxValue)
		        {
			        values.Add(MaxValue);
			        value -= MaxValue;
		        }
		        values.Add(value);
	        }
            
	        // Perform delta encoding
            List<int> deltaValues = new List<int>();
         
            AddValue(indices[0], deltaValues);

            for (int i = 1; i < indices.Length; ++i)
            {
                int currentDiff = indices[i] - indices[i - 1];

                AddValue(currentDiff, deltaValues);
            }

            // Compress
            streamWriter.Reset();

            for (int i = 0; i < deltaValues.Count; ++i)
            {
	            int bitIndex; // Figure out bit rate

	            if (deltaValues[i]  <= 1)        bitIndex = 0;
	            else if (deltaValues[i]  <= 3)   bitIndex = 1;
	            else if (deltaValues[i]  <= 7)   bitIndex = 2;
	            else if (deltaValues[i]  <= 15)  bitIndex = 3;
	            else throw new Exception("Hmm");
	            
	            streamWriter.Write((uint)bitIndex, HeaderBitSize);
	            streamWriter.Write((uint)deltaValues[i], (int) BITS[bitIndex]);
            }
            
            streamWriter.Flush();

            byte[] finalCompressedData = new byte[streamWriter.Length];
            System.Array.Copy(streamWriter.Buffer, finalCompressedData, streamWriter.Length);
            
            data[index] = new VisibilitySet()
            {
                compressed = finalCompressedData,
                len = (ushort)deltaValues.Count
            };
            
            if (PerfectCullingConstants.SafetyChecks)
            {
	            List<ushort> test = new List<ushort>();
	            SampleAtIndex(index, test, streamReader);

	            if (indices.Length != test.Count)
	            {
		            throw new Exception("Length mismatch: " + indices.Length + " vs " + test.Count);
	            }
	            
	            for (int i = 0; i < indices.Length; ++i)
	            {
		            if (indices[i] != test[i])
		            {
			            throw new Exception("Mismatch " + indices[i] + " " + test[i] + " @ index " + index + " : " + i + " - Maybe the array was unsorted?");
		            }
	            }
            }
        }
        
        public override void CompleteBake()
        {
	        if (rawData == null || rawData.Length <= 0)
	        {
		        return;
	        }
	        
	        // We only compress the data after we finished the entire bake.
	        // This prevents compressing and uncompressing unnecessarily during post processing steps.
	        data = new VisibilitySet[rawData.Length];

	        const int batchSize = 32;

	        int totalCount = rawData.Length;
	        
	        IEnumerable<IGrouping<int, int>> batches = Enumerable.Range(0, totalCount)
		        .GroupBy(val => (val % batchSize));

	        int processedElementCount = 0;

	        var setDataTasks = batches.Select(groups =>
	        {
#pragma warning disable 1998
		        return Task.Run(async () =>
#pragma warning restore 1998
		        {
			        BitStreamReader bitStreamReader = new BitStreamReader();
			        BitStreamWriter bitStreamWriter = new BitStreamWriter();
			        
			        int groupSize = 0;

			        foreach (var index in groups)
			        {
				        System.Array.Sort(rawData[index].uncompressed);
				        
				        SetDataCompressed(index, rawData[index].uncompressed, bitStreamReader, bitStreamWriter);
				        
				        ++groupSize;
			        }

			        System.Threading.Interlocked.Add(ref processedElementCount, groupSize);
		        });
	        });

	        var task = Task.WhenAll(setDataTasks);
	        var taskAwaiter = task.GetAwaiter();

	        for (int currentValue = 0; currentValue != totalCount; currentValue = System.Threading.Interlocked.CompareExchange(ref processedElementCount, 0, 0))
	        {
#if UNITY_EDITOR
		        UnityEditor.EditorUtility.DisplayProgressBar("Compress and apply cell data",$"Cell: {currentValue}/{totalCount}", currentValue / (float) totalCount);
#endif
		        
		        if (task.Wait(500))
		        {
			        // Task finished within timeout and we are done.
			        break;
		        }
	        }
	        
	        System.Diagnostics.Debug.Assert(processedElementCount == totalCount);

	        // Unnecessary but just for completeness.
	        taskAwaiter.GetResult();
	        
	        /*
	        for (int i = 0; i < rawData.Length; ++i)
	        {
		        PerfectCullingTemp.ListUshort.Clear();
		        PerfectCullingTemp.ListUshort.AddRange(rawData[i].uncompressed);
		        PerfectCullingTemp.ListUshort.Sort();
		        SetDataCompressed(i, PerfectCullingTemp.ListUshort.ToArray());
	        }
	        */

	        rawData = null;
        }

        /// <summary>
        /// Adds neighbor cell content to each cell then downsamples the entire grid.
        /// </summary>
        public void MergeDownsample()
        {
	        if (cellCount.x == 1 && cellCount.y == 1 && cellCount.z == 1)
	        {
		        Debug.LogWarning("Unable to downsample any further.");
		        
		        return;
	        }
	        
	        PerfectCullingVolumeBakeData tmpBakeData = ScriptableObject.CreateInstance<PerfectCullingVolumeBakeData>();

	        // Half resolution; divide by two
	        Vector3Int OptimizedCellSize = new Vector3Int(2, 2, 2);
	        
	        Vector3Int newTmpDim = new Vector3Int(
		        (int)(    (cellCount.x % OptimizedCellSize.x == 0)
			        ? cellCount.x 
			        : cellCount.x + OptimizedCellSize.x - (cellCount.x % OptimizedCellSize.x)),
                
		        (int)(  (cellCount.y % OptimizedCellSize.y == 0)
			        ? cellCount.y 
			        : cellCount.y + OptimizedCellSize.y - (cellCount.y % OptimizedCellSize.y)),
                
		        (int)( (cellCount.z % OptimizedCellSize.z == 0)
			        ? cellCount.z 
			        : cellCount.z + OptimizedCellSize.z - (cellCount.z % OptimizedCellSize.z)));
	        
	        Vector3Int optDim = new Vector3Int(
		        ((int)newTmpDim.x / (int)OptimizedCellSize.x),
		        ((int)newTmpDim.y / (int)OptimizedCellSize.y),
		        ((int)newTmpDim.z / (int)OptimizedCellSize.z));

	        //tmpBakeData.data = new VisibilitySet[optDim.x * optDim.y * optDim.z];
	        tmpBakeData.rawData = new RawData[optDim.x * optDim.y * optDim.z];
	        
	        tmpBakeData.cellCount = optDim;;
	        tmpBakeData.cellSize = new Vector3(cellSize.x * OptimizedCellSize.x, cellSize.y*OptimizedCellSize.y,
		        cellSize.z *OptimizedCellSize.z);

	        int totalCount = Mathf.CeilToInt(cellCount.x * cellCount.y * cellCount.z);

	        
#if false
	        HashSet<ushort> tmpHash = new HashSet<ushort>();
	        
	        for (int index = 0; index < totalCount; ++index)
	        {
		        PerfectCullingMath.UnflattenToXYZ(index, out int x, out int y, out int z, cellCount);

		        int optX = x / (int)OptimizedCellSize.x;
		        int optY = y / (int)OptimizedCellSize.y;
		        int optZ = z / (int)OptimizedCellSize.z;
		        
		        int tmpBakeDataSampleIndex = PerfectCullingMath.FlattenXYZ(optX, optY, optZ, optDim);
		        
		        tmpHash.Clear();
		        
		        // Merge neighbor cells
		        for (int xx = -1; xx <= 1; ++xx)
		        {
			        for (int yy = -1; yy <= 1; ++yy)
			        {
				        for (int zz = -1; zz <= 1; ++zz)
				        {
					        if (!PerfectCullingMath.IsXYZInBounds(x + xx, y + yy, z + zz, cellCount))
					        {
						        continue;
					        }

					        int sampleIndex = PerfectCullingMath.FlattenXYZ(x + xx, y + yy, z + zz, cellCount);

					        if (rawData[sampleIndex].uncompressed == null)
					        {
						        continue;
					        }
					        
					        foreach (ushort neighborIndex in rawData[sampleIndex].uncompressed)
					        {
						        tmpHash.Add(neighborIndex);
					        }
				        }
			        }
		        }
		        
		        // Add existing indices back in or they would be lost
		        if (tmpBakeData.rawData[tmpBakeDataSampleIndex].uncompressed != null)
		        {
			        foreach (ushort existingIndex in tmpBakeData.rawData[tmpBakeDataSampleIndex].uncompressed)
			        {
				        tmpHash.Add(existingIndex);
			        }
		        }

		        tmpBakeData.rawData[tmpBakeDataSampleIndex].uncompressed = tmpHash.ToArray();
		        
		        
#if UNITY_EDITOR
		        if (index % 128 == 0)
		        {
			        UnityEditor.EditorUtility.DisplayProgressBar("Performing Merge-Downsample step",
				        $"Cell: {index}/{totalCount}", index / (float) totalCount);
		        }
#endif
	        }
#else
	        const int batchSize = 32;

	        IEnumerable<IGrouping<int, int>> batches = Enumerable.Range(0, totalCount)
		        .GroupBy(val => (val % batchSize));

	        int processedElementCount = 0;

	        var downsampleTasks = batches.Select(groups =>
	        {
#pragma warning disable 1998
		        return Task.Run(async () =>
#pragma warning restore 1998
		        {
			        HashSet<ushort> tmpHash = new HashSet<ushort>();

			        int groupSize = 0;
			        
			        foreach (var index in groups)
			        {
				        PerfectCullingMath.UnflattenToXYZ(index, out int x, out int y, out int z, cellCount);

				        int optX = x / (int)OptimizedCellSize.x;
				        int optY = y / (int)OptimizedCellSize.y;
				        int optZ = z / (int)OptimizedCellSize.z;
		        
				        int tmpBakeDataSampleIndex = PerfectCullingMath.FlattenXYZDouble(optX, optY, optZ, optDim);
		        
				        tmpHash.Clear();
		        
				        // Merge neighbor cells
				        for (int xx = -1; xx <= 1; ++xx)
				        {
					        for (int yy = -1; yy <= 1; ++yy)
					        {
						        for (int zz = -1; zz <= 1; ++zz)
						        {
							        if (!PerfectCullingMath.IsXYZInBounds(x + xx, y + yy, z + zz, cellCount))
							        {
								        continue;
							        }

							        int sampleIndex = PerfectCullingMath.FlattenXYZDouble(x + xx, y + yy, z + zz, cellCount);

							        if (rawData[sampleIndex].uncompressed == null)
							        {
								        continue;
							        }
					        
							        foreach (ushort neighborIndex in rawData[sampleIndex].uncompressed)
							        {
								        tmpHash.Add(neighborIndex);
							        }
						        }
					        }
				        }
		        
				        // Add existing indices back in or they would be lost
				        if (tmpBakeData.rawData[tmpBakeDataSampleIndex].uncompressed != null)
				        {
					        foreach (ushort existingIndex in tmpBakeData.rawData[tmpBakeDataSampleIndex].uncompressed)
					        {
						        tmpHash.Add(existingIndex);
					        }
				        }

				        tmpBakeData.rawData[tmpBakeDataSampleIndex].uncompressed = tmpHash.ToArray();

				        ++groupSize;
			        }

			        System.Threading.Interlocked.Add(ref processedElementCount, groupSize);

		        });
	        });

	        var task = Task.WhenAll(downsampleTasks);
	        var taskAwaiter = task.GetAwaiter();

	        for (int currentValue = 0; currentValue != totalCount; currentValue = System.Threading.Interlocked.CompareExchange(ref processedElementCount, 0, 0))
	        {
#if UNITY_EDITOR
		        UnityEditor.EditorUtility.DisplayProgressBar("Performing Merge-Downsample step",
			        $"Cell: {currentValue}/{totalCount}", currentValue / (float) totalCount);
#endif
		        
		        if (task.Wait(500))
		        {
			        // Task finished within timeout and we are done.
			        break;
		        }
	        }
	        
	        System.Diagnostics.Debug.Assert(processedElementCount == totalCount);

	        // Unnecessary but just for completeness.
	        taskAwaiter.GetResult();
#endif

	        rawData = tmpBakeData.rawData;
	        cellCount = tmpBakeData.cellCount;
	        cellSize = tmpBakeData.cellSize;
	        
	        GameObject.DestroyImmediate(tmpBakeData);
        }

        // This is called at run-time and needs to be as efficient as possible
        public override void SampleAtIndex(int index, List<ushort> indices)
        {
	        SampleAtIndex(index, indices, m_bitStreamReader);
        }

        void SampleAtIndex(int index, List<ushort> indices, BitStreamReader bitStreamReader)
        {
	        bitStreamReader.Reset(data[index].compressed);

	        ushort accumulator = 0;

	        int len = data[index].len;
	        
	        for (int i = 0; i < len; ++i)
	        {
		        uint bitIndex = bitStreamReader.Read(HeaderBitSize);

		        uint numberOfBits = BITS[bitIndex];
		        uint value = bitStreamReader.Read((int) numberOfBits);

		        accumulator += (ushort) value; 
		        
		        if (value >= MaxValue)
		        {
			        continue;
		        }

		        indices.Add(accumulator);
	        }
        }
        
        // Called at run-time to scan for closest non-empty cell
        public override int SearchIndexForClosestNonEmptyCell(int index)
        {
	        if (data[index].len > 0)
	        {
		        return index;
	        }

	        PerfectCullingMath.UnflattenToXYZ(index, out int x, out int y, out int z, cellCount);

	        int smallestDist = int.MaxValue;
	        int resultIndex = -1;
	        
	        for (int xx = -PerfectCullingConstants.MaxNonEmptyCellSearchRange; xx <= PerfectCullingConstants.MaxNonEmptyCellSearchRange; ++xx)
	        {
		        for (int yy = -PerfectCullingConstants.MaxNonEmptyCellSearchRange; yy <= PerfectCullingConstants.MaxNonEmptyCellSearchRange; ++yy)
		        {
			        for (int zz = -PerfectCullingConstants.MaxNonEmptyCellSearchRange; zz <= PerfectCullingConstants.MaxNonEmptyCellSearchRange; ++zz)
			        {
				        if (!PerfectCullingMath.IsXYZInBounds(x + xx, y + yy, z + zz, cellCount))
				        {
					        continue;
				        }

				        int sampleIndex = PerfectCullingMath.FlattenXYZDouble(x + xx, y + yy, z + zz, cellCount);

				        if (data[sampleIndex].len > 0)
				        {
					        int dist = (xx * xx) + (yy * yy) + (zz * zz);

					        if (smallestDist > dist)
					        {
						        smallestDist = dist;
						        resultIndex = sampleIndex;
					        }
				        }
			        }
		        }
	        }
	        
	        return resultIndex;
        }
        
        private static BitStreamReader m_bitStreamReader = new BitStreamReader();

        public override void DrawInspectorGUI()
        {
#if UNITY_EDITOR
	        EditorGUILayout.Vector3Field($"Cell size", cellSize);
	        EditorGUILayout.Vector3Field($"Cell count", cellCount);
	        EditorGUILayout.Toggle($"Bake completed", bakeCompleted);

	        EditorGUILayout.IntField("Data cells", data.Length);
#endif
        }
    }
}