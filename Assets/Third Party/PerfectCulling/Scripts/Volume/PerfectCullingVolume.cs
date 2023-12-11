// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingVolume : PerfectCullingBakingBehaviour, CustomHandle.IResizableByHandle
    {
        [FormerlySerializedAs("VolumeSize")] [SerializeField] public Vector3 volumeSize = Vector3.one;

        [SerializeField]
        public Bounds volumeBakeBounds
        {
            get => new Bounds(transform.position, volumeSize);

            set
            {
                // TODO: Causes annoying offset, gonna need to solve this in a different way.
                transform.position = value.center;
                
                volumeSize = new Vector3(
                    Mathf.Max(1, value.size.x), 
                    Mathf.Max(1, value.size.y), 
                    Mathf.Max(1, value.size.z));
            }
        }
        
        public int CellCount => PerfectCullingMath.CalculateNumberOfCells(volumeSize, bakeCellSize);
        public bool VisualizeProbes { get; set; }
        public bool VisualizeGridCells { get; set; }
        public bool VisualizeHitLines { get; set; }
        public int RenderersCount => bakeGroups.Length;

        public static readonly List<PerfectCullingVolume> AllVolumes = new List<PerfectCullingVolume>();

        [FormerlySerializedAs("VolumeBakeData")] public PerfectCullingVolumeBakeData volumeBakeData;

        public override PerfectCullingBakeData BakeData => volumeBakeData;

        [FormerlySerializedAs("BakeCellSize")]
        [Tooltip("The size of a single cell. This needs to be a divisor of the scale of the volume.")]
        [SerializeField] public Vector3 bakeCellSize = new Vector3(10, 5, 10);

        [FormerlySerializedAs("MergeDownsampleIterations")]
        [Tooltip("After the bake completed for each cell all neighbor cells are merged into a single cell. This will reduce the number of cells without introducing culling issues. This is useful to reduce memory usage.")]
        [Range(0, 8)]
        public int mergeDownsampleIterations = 0;
        
        public PerfectCullingVisibilityLayer visibilityLayer = PerfectCullingVisibilityLayer.All;
        
        private void OnEnable()
        {
            // We got no baked data. We are useless and shouldn't even bother adding ourselves to the list.
            if (volumeBakeData == null || volumeBakeData.data.Length <= 0)
            {
                return;
            }
            
            AllVolumes.Add(this);
        }

        private void OnDisable()
        {
            // Execute this before ToggleAllRenderers - just in case we run into an exception.
            AllVolumes.Remove(this);
            
            // We turn everything back on because it might still be disabled from the previous frame.
            QueueToggleAllRenderers(true);
            
            // Take effect immediately
            // We also force null checks because OnDisable() might have been called as part of an active destruction process (scene change, etc.)
            ExecuteQueue(true);
        }
        
        private void OnDestroy()
        {
            // Makes sure that we don't keep this object alive
            volumeBakeData = null;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            Vector3 nScale = new Vector3((int)volumeSize.x, (int)volumeSize.y, (int)volumeSize.z);

            // Make sure the scale of the object actually supports this cell size
            bakeCellSize = new Vector3(
                Mathf.Min(bakeCellSize.x, nScale.x), 
                Mathf.Min(bakeCellSize.y, nScale.y), 
                Mathf.Min(bakeCellSize.z, nScale.z));
        }

        private void OnDrawGizmos()
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Matrix4x4 newMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            
            Gizmos.matrix = newMatrix;
            Handles.matrix = newMatrix;
            
            //Gizmos.color = Color.blue;
            //Gizmos.DrawWireCube(Vector3.zero, volumeSize * 1.01f);
            
            if (!VisualizeProbes && !VisualizeGridCells)
            {
                return;
            }

            Vector3 cellCount = PerfectCullingMath.CalculateCellCount(volumeSize, bakeCellSize);

            Handles.color = new Color(0.5f, 0.25f, 0f, 0.35f);
            Handles.zTest = CompareFunction.LessEqual;
            
            InitializeAllSamplingProviders();
            
            foreach (Camera camera in SceneView.GetAllSceneCameras())
            {
                int camSampleIndex = GetIndexForWorldPos(camera.transform.position, bakeCellSize, out bool _);

                PerfectCullingMath.UnflattenToXYZ(camSampleIndex, out int x, out int y, out int z, cellCount);

                const int Radius = 6;
                
                for (int xx = -Radius; xx < Radius; ++xx)
                {
                    for (int yy = -Radius; yy < Radius; ++yy)
                    {
                        for (int zz = -Radius; zz < Radius; ++zz)
                        {
                            if (!PerfectCullingMath.IsXYZInBounds(x + xx, y + yy, z + zz, cellCount))
                            {
                                continue;
                            }
                            
                            int neighborCellIndex = PerfectCullingMath.FlattenXYZ(x + xx, y + yy, z + zz, cellCount);
                            
                            Vector3 pos = GetSamplingPositionAt(neighborCellIndex, bakeCellSize);

                            if (VisualizeGridCells)
                            {
                                Handles.DrawWireCube(pos, bakeCellSize);
                            }

                            if (!SamplingProvidersIsPositionActive(
                                GetSamplingPositionAt(neighborCellIndex, bakeCellSize, Space.World)))
                            {
                                continue;
                            }

                            if (VisualizeProbes)
                            {
                                Handles.SphereHandleCap(-1, pos, Quaternion.identity, 0.5f, EventType.Repaint);
                            }
                        }
                    }
                }
            }

            if (volumeBakeData == null)
            {
                return;
            }

            foreach (PerfectCullingCamera camera in PerfectCullingCamera.AllCameras)
            {
                int i = GetIndexForWorldPos(camera.transform.position, bakeCellSize, out bool _);

                Vector3 pos = GetSamplingPositionAt(i, bakeCellSize);
                
                Gizmos.color = new Color(0.5f, 0f, 0f, 0.5f);
                
                Gizmos.DrawCube(pos, bakeCellSize);
                
                Gizmos.color = new Color(0.0f, 0.25f, 0f, 0.5f);

                Gizmos.matrix = oldMatrix;

                if (bakeCellSize == volumeBakeData.cellSize)
                {
                    PerfectCullingTemp.ListUshort.Clear();
                    BakeData.SampleAtIndex(i, PerfectCullingTemp.ListUshort);
                    foreach (ushort index in PerfectCullingTemp.ListUshort)
                    {
                        if (!VisualizeHitLines)
                        {
                            break;
                        }
                        
                        bakeGroups[index].ForeachRenderer((renderer) =>
                        {
                            Gizmos.DrawLine(transform.rotation * pos + transform.position, renderer.bounds.center);
                        });
                    }
                }
                else
                {
                    int otherI = GetIndexForWorldPos(camera.transform.position, volumeBakeData.cellCount, volumeBakeData.cellSize, out bool _);
                    PerfectCullingTemp.ListUshort.Clear();
                    BakeData.SampleAtIndex(otherI, PerfectCullingTemp.ListUshort);
                    foreach (ushort index in PerfectCullingTemp.ListUshort)
                    {
                        if (!VisualizeHitLines)
                        {
                            break;
                        }
                        
                        bakeGroups[index].ForeachRenderer((renderer) =>
                        {
                            Gizmos.DrawLine(transform.rotation * pos + transform.position, renderer.bounds.center);
                        });
                    }

                    Vector3 otherPos = GetSamplingPositionAt(otherI, volumeBakeData.cellCount, volumeBakeData.cellSize);
                    
                    Vector3 bakedVolumeSize = Vector3.Scale(volumeBakeData.cellCount, volumeBakeData.cellSize);
                    Vector3 diff = volumeSize - bakedVolumeSize;
                    
                    Gizmos.color = new Color(1, 1, 0, 0.2f);
                    Gizmos.matrix = newMatrix;
                    Gizmos.DrawCube(otherPos - 0.5f * diff, volumeBakeData.cellSize);
                    Gizmos.matrix = oldMatrix;
                }

                Gizmos.matrix = newMatrix;
            }
        }
#endif
        
        public override void GetIndicesForWorldPos(Vector3 worldPos, List<ushort> indices)
        { 
            int flat = GetIndexForWorldPos(worldPos, volumeBakeData.cellSize, out bool _);

            GetIndicesForIndex(flat, indices);
        }

        public override List<Vector3> GetSamplingPositions(Space space = Space.Self)
        {
            List<Vector3> samplePositions = new List<Vector3>(CellCount);
            
            for (int rawIndexCell = 0; rawIndexCell < CellCount; ++rawIndexCell)
            {
                samplePositions.Add(GetSamplingPositionAt(rawIndexCell, bakeCellSize, space));
            }

            return samplePositions;
        }

        public override bool PreBake()
        {
            // Needs to be an integer for clean divisions
            volumeSize = new Vector3(
                (int) volumeSize.x, 
                (int) volumeSize.y,
                (int) volumeSize.z);
            
            volumeBakeData.SetVolumeBakeData(this);

            if ((int)volumeBakeData.cellCount.x == 0 || (int)volumeBakeData.cellCount.y == 0 || (int)volumeBakeData.cellCount.z == 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Invalid cell size.",
                    "The cell size is invalid. Please double check that the scale of your volume supports at least one cell of the given size.",
                    "OK");
#endif
                
                return false;
            }

            return true;
        }

        protected override void CullAdditionalOccluders(ref HashSet<Renderer> additionalOccluders)
        {
            if (additionalOccluders == null)
            {
                return;
            }

            Bounds bakeBounds = new Bounds(transform.position, volumeSize);

            HashSet<Renderer> relevantOccluders = new HashSet<Renderer>();

            foreach (Renderer r in additionalOccluders)
            {
                if (!bakeBounds.Intersects(r.bounds))
                {
                    continue;
                }

                relevantOccluders.Add(r);
            }

            additionalOccluders = relevantOccluders;
        }

        public override void PostBake()
        {
            for (int i = 0; i < mergeDownsampleIterations; ++i)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar(
                    "Performing Merge-Downsample step", $"Performing Merge-Downsample iteration {i + 1}/{mergeDownsampleIterations}",
                    i / (float) mergeDownsampleIterations);
#endif
                volumeBakeData.MergeDownsample();
            }
        }

        public Vector3 AlignToGrid(Vector3 pos)
        { 
            int idx = GetIndexForWorldPos(pos, bakeCellSize, out bool _);
            
            return GetSamplingPositionAt(idx, bakeCellSize, Space.World);
        }
        
        public override int GetIndexForWorldPos(Vector3 pos, out bool isOutOfBounds)
        {
            return GetIndexForWorldPos(pos, volumeBakeData.cellSize, out isOutOfBounds);
        }
        
        private int GetIndexForWorldPos(Vector3 pos, Vector3 cellSize, out bool isOutOfBounds)
        {
            Vector3 cellCount = PerfectCullingMath.CalculateCellCount(volumeSize, cellSize);
            
            return GetIndexForWorldPos(pos, cellCount, cellSize, out isOutOfBounds);
        }

        private List<PerfectCullingPortalCell> portalCells = new List<PerfectCullingPortalCell>();
        public void AddPortalCell(PerfectCullingPortalCell portalCell) => portalCells.Add(portalCell);
        public void RemovePortalCell(PerfectCullingPortalCell portalCell) => portalCells.Remove(portalCell);

        private int GetIndexForWorldPos(Vector3 pos, Vector3 cellCount, Vector3 cellSize, out bool isOutOfBounds)
        {
            Quaternion orientation = volumeBakeData == null ? transform.rotation : volumeBakeData.orientation;
            
            int index = PerfectCullingMath.GetIndexForWorldPos(pos, transform.position, transform.rotation,
                volumeSize, orientation, cellCount, cellSize, out isOutOfBounds);

            if (isOutOfBounds)
            {
                PerfectCullingPortalCell closest = null;
                float dist = float.MaxValue;
                
                foreach (PerfectCullingPortalCell cell in portalCells)
                {
                    float thisDist = (pos - cell.transform.position).sqrMagnitude;

                    if (dist > thisDist)
                    {
                        closest = cell;
                        dist = thisDist;
                    }
                }

                if (closest != null)
                {
                    return PerfectCullingMath.GetIndexForWorldPos(closest.transform.position, transform.position, transform.rotation,
                        volumeSize, orientation, cellCount, cellSize, out isOutOfBounds);
                }
            }

            if (searchForNonEmptyCells)
            {
                int newIndex = volumeBakeData.SearchIndexForClosestNonEmptyCell(index);

                if (newIndex != -1)
                {
                    index = newIndex;
                }
            }

            return index;
        }

        public override void SetBakeData(PerfectCullingBakeData bakeData) => volumeBakeData = bakeData as PerfectCullingVolumeBakeData;
        
        private Vector3 GetSamplingPositionAt(int index, Vector3 cellSize, Space space = Space.Self)
        {
            Vector3 gridSize = new Vector3(volumeSize.x / cellSize.x,
                volumeSize.y / cellSize.y, volumeSize.z / cellSize.z);

            return GetSamplingPositionAt(index, gridSize, cellSize, space);
        }
        
        private Vector3 GetSamplingPositionAt(int index, Vector3 gridSize, Vector3 cellSize, Space space = Space.Self)
        {
            Vector3 halfGridSize = gridSize * 0.5f;
            
            PerfectCullingMath.UnflattenToXYZ(index, out int x, out int y, out int z, gridSize);

            Vector3 localPos = (cellSize / 2 +
                new Vector3(x * cellSize.x, y * cellSize.y, z * cellSize.z) - new Vector3(
                    halfGridSize.x * cellSize.x, halfGridSize.y * cellSize.y, halfGridSize.z * cellSize.z));
            
            if (space == Space.World)
            {
                return transform.position + transform.rotation * localPos;

            }

            return localPos;
        }

        public Vector3 HandleSized
        {
            get => volumeBakeBounds.size;
            set => volumeBakeBounds = new Bounds(transform.position, value);
        }

        public override int GetBakeHash()
        {
            int hash = 13;

            unchecked
            {
                hash = hash * 17 + bakeGroups.Length;

                for (int i = 0; i < bakeGroups.Length; ++i)
                {
                    hash = hash * 53 + (int)bakeGroups[i].groupType;
                    hash = hash * 23 + bakeGroups[i].renderers.Length;
                }

                hash = hash * 41 + additionalOccluders.Count;
            }

            return hash;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // We draw this here so it will render during multi-selection

            Matrix4x4 trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            
            Handles.color = new Color(0, 0, 1, 1.0f);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.lighting = false;
            
            Handles.matrix = trs;
            Handles.DrawWireCube(Vector3.zero, volumeSize);
            
            // CubeHandleCap doesn't allow to specify non-uniform scale so we need to mess with the matrix
            Handles.matrix = trs * Matrix4x4.Scale(volumeSize);

            Handles.color = new Color(0, 0, 1, PerfectCullingConstants.VolumeInsideAlpha);
            Handles.CubeHandleCap(-1, Vector3.zero, Quaternion.identity, 1.0f, EventType.Repaint);
        }
#endif
    }
}