using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The camera to have control over")]
    public Camera controledCamera;
    [Tooltip("The speed at which the camera rotates to look up and down (calculated in degrees)")]
    public float rotationSpeed = 60f;
    [Tooltip("Whether or not to invert the look direction")]
    public bool invert = true;

    private InputManager inputManager;

    void Start()
    {
        SetUpCamera();
        SetUpInputManager();
    }

    int waitForFrames = 3;
    int framesWaited;
    void Update()
    {
        if (framesWaited <= waitForFrames)
        {
            framesWaited += 1;
            return;
        }
        ProcessRotation();
    }
    
    void SetUpCamera()
    {
        if (controledCamera == null)
        {
            controledCamera = GetComponent<Camera>();
        }
    }
    
    void SetUpInputManager()
    {
        inputManager = FindObjectOfType<InputManager>();
    }
    void ProcessRotation()
    {
        float verticalLookInput = inputManager.verticalLookAxis;
        Vector3 cameraRotation = controledCamera.transform.rotation.eulerAngles;
        float newXRotation = 0;
        if (invert)
        {
            newXRotation  = cameraRotation.x - verticalLookInput * rotationSpeed * Time.deltaTime;
        }
        else
        {
            newXRotation = cameraRotation.x + verticalLookInput * rotationSpeed * Time.deltaTime;
        }

        if (newXRotation < 270 && newXRotation >= 180)
        {
            newXRotation = 270;
        }
        else if (newXRotation > 90 && newXRotation < 180)
        {
            newXRotation = 90;
        }
        controledCamera.transform.rotation = Quaternion.Euler(new Vector3(newXRotation, cameraRotation.y, cameraRotation.z));
    }
}
