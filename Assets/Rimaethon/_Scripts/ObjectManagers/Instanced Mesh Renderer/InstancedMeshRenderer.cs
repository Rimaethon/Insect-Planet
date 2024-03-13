using System;
using Insect_Planet._Scripts.ObjectManagers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public class InstancedMeshRenderer: MonoBehaviour
{
    #region Fields
    public string fileName;

    [SerializeField] private Material objectMaterial;
    [SerializeField] private Mesh[] meshesToRender;
    [SerializeField] private ComputeShader[] computeShaders;
    public Vector3 quad;
    private int _instanceCount = 15000;
    private float3[] _meshSpawnPositions;
    private Bounds _bounds;
    public int _kernel;
    public int asteroidKernel;
    [SerializeField] private Camera _camera;
    private Transform _transform;
    private Transform _cameraTransform;
    [SerializeField] private int LOD1Distance;
    [SerializeField] private int LOD2Distance;
    [SerializeField] private int LOD3Distance;

    //Buffers

    // 30 instances need 30 compute shaders
    private ComputeShader[] _computeShaderInstances;

    //Scene Positions as float3[]
    private ComputeBuffer _meshSpawnPositionsBuffer;

    // Start index, vertices count, instance count, base vertex data
    private ComputeBuffer[] _meshArgumentBuffers;

    //Positions of vertices for each mesh its length is instancecount*verticecount
    private ComputeBuffer[] _vertexWorldPositionsOutputBuffers;

    //For sending original vertex positions to compute shader
    private ComputeBuffer[] _vertexLocalPositionsBuffers;

    private MaterialPropertyBlock[] _materialPropertyBlocks;

    #endregion

    private void Start()
    {
        GetReferences();
    }

    private void GetReferences()
    {
        _meshSpawnPositions = FlattenArray(Vector3Serializer.LoadFromFile(fileName));
        _instanceCount= _meshSpawnPositions.Length;
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        _transform = transform;
        _bounds = new Bounds(_transform.position, new Vector3(300, 130, 250));


        if (meshesToRender==null)
        {
            Debug.LogError("There are no meshes to render");
        }
        else
        {
            InitializeBuffers();
        }
    }

    private void OnDisable()
    {
        ReleaseBuffers();
    }
    private void ReleaseBuffers()
    {
        if (_meshArgumentBuffers!=null)
        {
            foreach (var buffer in _meshArgumentBuffers)
            {
                buffer?.Release();
            }

        }

        if (_vertexLocalPositionsBuffers!=null)
        {
            foreach (var buffer in _vertexLocalPositionsBuffers)
            {
                buffer?.Release();
            }
        }

        if (_vertexWorldPositionsOutputBuffers!=null)
        {
            foreach (var buffer in _vertexWorldPositionsOutputBuffers)
            {
                buffer?.Release();
            }
        }

        _meshSpawnPositionsBuffer?.Release();
        _vertexLocalPositionsBuffers[0]?.Release();

    }



    private void InitializeBuffers()
    {
        _meshArgumentBuffers = new ComputeBuffer[meshesToRender.Length];
        _materialPropertyBlocks = new MaterialPropertyBlock[meshesToRender.Length];
        _vertexLocalPositionsBuffers = new ComputeBuffer[meshesToRender.Length];
        _vertexWorldPositionsOutputBuffers = new ComputeBuffer[meshesToRender.Length];

        _meshSpawnPositionsBuffer = new ComputeBuffer(_instanceCount,sizeof(float)*3);
        _meshSpawnPositionsBuffer.SetData(_meshSpawnPositions);
        _computeShaderInstances = new ComputeShader[meshesToRender.Length];

        _kernel = 0;
        for (int i = 0; i < meshesToRender.Length; i++)
        {

            _computeShaderInstances[i] = Instantiate(computeShaders[i]);

            _materialPropertyBlocks[i] = new MaterialPropertyBlock();
            _meshArgumentBuffers[i] = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            _meshArgumentBuffers[i].SetData(GetArgsBuffer((uint)_instanceCount,meshesToRender[i]));

            _vertexLocalPositionsBuffers[i]= new ComputeBuffer(meshesToRender[i].vertexCount, 3 * 4);
            _vertexLocalPositionsBuffers[i].SetData(meshesToRender[i].vertices);

            _vertexWorldPositionsOutputBuffers[i] =
                new ComputeBuffer(meshesToRender[i].vertexCount * _instanceCount, 3 * 4);

            _computeShaderInstances[i].SetInt("vertexCount",meshesToRender[i].vertexCount);
            _computeShaderInstances[i].SetBuffer(_kernel,"VertexPositionsInput", _vertexLocalPositionsBuffers[i]);
            _computeShaderInstances[i].SetBuffer(_kernel,"SpawnPositions", _meshSpawnPositionsBuffer);
            _computeShaderInstances[i].SetBuffer(_kernel,"VertexPositionsOutput",_vertexWorldPositionsOutputBuffers[i]);


            _computeShaderInstances[i].Dispatch(_kernel,_instanceCount/32,1,1);

            _materialPropertyBlocks[i].SetInt("vertexCount",meshesToRender[i].vertexCount);
            _materialPropertyBlocks[i].SetBuffer("VertexPositionsOutput",_vertexWorldPositionsOutputBuffers[i]);

            if (i == 0)
            {
                _materialPropertyBlocks[i].SetFloat("_Intensity",5.5f);
            }


        }
    }

    public bool IsObjectVisible()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        return GeometryUtility.TestPlanesAABB(planes, _bounds);
    }
    public int LODIndex=0;
    private void Update()
    {
        if(!IsObjectVisible())
            return;

        if (Vector3.Distance(transform.position,_camera.transform.position)<LOD3Distance)
        {
            LODIndex = 3;

        }else if (Vector3.Distance(transform.position,_camera.transform.position)<LOD2Distance)
        {
            LODIndex = 2;
        }else if (Vector3.Distance(transform.position,_camera.transform.position)<LOD1Distance)
        {
            LODIndex = 1;
        }
        else
        {
            LODIndex = 0;
        }

        if (LODIndex==0)
        {
            gameObject.transform.LookAt(_cameraTransform.position);
            _computeShaderInstances[LODIndex].SetFloat("rotX",gameObject.transform.rotation.eulerAngles.x);
            _computeShaderInstances[LODIndex].SetFloat("rotY",gameObject.transform.rotation.eulerAngles.y);
            _computeShaderInstances[LODIndex].SetFloat("rotZ",gameObject.transform.rotation.eulerAngles.z);
            _computeShaderInstances[LODIndex].Dispatch(_kernel,_instanceCount/32,1,1);
        }
        else
        {
            _computeShaderInstances[LODIndex].Dispatch(_kernel,_instanceCount/32,1,1);

        }
        Graphics.DrawMeshInstancedIndirect(meshesToRender[LODIndex], 0, objectMaterial, _bounds, _meshArgumentBuffers[LODIndex], 0, _materialPropertyBlocks[LODIndex], ShadowCastingMode.Off, false, 0, _camera);

    }


    private uint[] GetArgsBuffer(uint count, Mesh mesh)
    {
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        args[0] = mesh.GetIndexCount(0);
        args[1] = count;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        return args;
    }
    private float3[] FlattenArray(Int16[][] array)
    {
        float3[] flattenedArray = new float3[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            flattenedArray[i]=new float3(array[i][0],array[i][1],array[i][2]);

        }
        return flattenedArray;
    }


}
