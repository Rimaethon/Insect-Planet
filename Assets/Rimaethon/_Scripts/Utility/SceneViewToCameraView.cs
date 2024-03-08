#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class SceneViewToCameraView : MonoBehaviour
{
    [SerializeField] private SceneView sceneView;
    [SerializeField] private Camera virtualCamera;
    [SerializeField] private bool _isSceneViewActive;
    [SerializeField] private Camera _camera;
    [SerializeField] private List<Transform> _transformsToMove;
    private void Awake()
    {
        sceneView = SceneView.lastActiveSceneView;
     
    }

    void Start()
    {
        

    }
    void Update()
    {
        if (_isSceneViewActive)
        {
           
            MoveCamera();
        }

    }

    private void MoveVirtualCamera()
    {
        virtualCamera.transform.position = sceneView.camera.transform.position;  
        virtualCamera.transform.rotation = sceneView.camera.transform.rotation;
        
    }
    
    private void MoveCamera()
    {
        _camera.transform.position = sceneView.camera.transform.position;  
        _camera.transform.rotation = sceneView.camera.transform.rotation;
        
    }
}
#endif
