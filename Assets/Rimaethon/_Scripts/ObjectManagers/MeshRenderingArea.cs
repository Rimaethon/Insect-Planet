using System.Collections.Generic;
using Insect_Planet._Scripts.ObjectManagers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshRenderingArea : MonoBehaviour
{
    private List<Vector3> _spawnPoints;
    [SerializeField] private Mesh objectMesh;
    [SerializeField] private Material objectMaterial;
    [SerializeField] int pointsToGenerate = 120000;

    private RenderParams _rpRenderParams;
    private NativeArray<Vector3> vectorPositions;
    private NativeArray<Matrix4x4> matrices;
    private bool isMatricesCached;
    private Matrix4x4[] _matricesCached;
    private int batchCount;
    private int batchSize = 1000;

    ComputeShader _computeShader;
    ComputeBuffer _computeBuffer;
    private void OnEnable()
    {
     _computeBuffer = new ComputeBuffer(pointsToGenerate, 12);


       // _spawnPoints = Vector3Serializer.LoadFromFile(fileName);
        vectorPositions = new NativeArray<Vector3>(pointsToGenerate, Allocator.Persistent);
        _matricesCached = new Matrix4x4[pointsToGenerate];
        matrices = new NativeArray<Matrix4x4>(pointsToGenerate, Allocator.Persistent);

        for (int i = 0; i < pointsToGenerate; i++)
        {
            vectorPositions[i] = _spawnPoints[i];
        }
        _rpRenderParams = new RenderParams(objectMaterial);
        batchCount =pointsToGenerate / batchSize;
    }

    private void OnDisable()
    {
        vectorPositions.Dispose();
        matrices.Dispose();
    }

    private void UpdateMatrices()
    {
        // Schedule the job to convert Vector3 positions to matrices in parallel
        var job = new Vector3ToMatrixJobImplementation
        {
            vectorPositions = vectorPositions,
            matrices = matrices
        };

        JobHandle jobHandle = job.Schedule(pointsToGenerate, 64); // You may need to adjust the batch size based on your specific requirements
        jobHandle.Complete();
        matrices.CopyTo(_matricesCached);
        isMatricesCached = true;
    }

    private void Start()
    {
        if (pointsToGenerate == matrices.Length)
        {
            UpdateMatrices();
        }
        else
        {
            Debug.LogError("Spawn points count does not match matrices length" + _spawnPoints.Count + " " + matrices.Length);
        }

        if (isMatricesCached)
        {
            for (int b = 0; b < batchCount; b++)
            {
                 Graphics.RenderMeshInstanced(_rpRenderParams, objectMesh, 0, _matricesCached, batchSize, b* batchSize);

            }

            for (int i = 0; i < pointsToGenerate; i++)
            {
                Graphics.RenderMesh(_rpRenderParams,objectMesh,0,_matricesCached[i]);
                Graphics.DrawMeshInstancedIndirect(objectMesh,0,objectMaterial,new Bounds(Vector3.zero,Vector3.one*1000),_computeBuffer);

            }
        }
        else if (pointsToGenerate == matrices.Length)
        {
            UpdateMatrices();
        }
        else
        {
            Debug.LogError("Spawn points count does not match matrices length" + _spawnPoints.Count + " " + matrices.Length);
        }
    }

    private void Update()
    {

    }

    [BurstCompile]
    private struct Vector3ToMatrixJobImplementation : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> vectorPositions;
        public NativeArray<Matrix4x4> matrices;

        public void Execute(int index)
        {
            Vector3 position = vectorPositions[index];
            matrices[index] = Matrix4x4.Translate(position);
        }
    }
}
