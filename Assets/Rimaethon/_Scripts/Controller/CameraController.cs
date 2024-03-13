using Rimaethon._Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    [SerializeField] private float lookSpeed = 1f;
    [SerializeField] private int downClamp = 30;
    [SerializeField] private int upClamp = -30;
    private Vector3 _moveVector;
    private Vector2 _lookVector;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerLook, HandlePlayerLook);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerLook, HandlePlayerLook);
    }

    private void HandlePlayerLook(Vector2 lookVector)
    {
        _lookVector = lookVector;
    }
    private float Xrotation;

    private void Look()
    {
        float horizontalLookInput = _lookVector.x * lookSpeed * Time.deltaTime;
        float verticalLookInput = _lookVector.y * lookSpeed * Time.deltaTime;
        Vector3 newRotation = transform.eulerAngles;
        Xrotation -= verticalLookInput;
        Xrotation = Mathf.Clamp(Xrotation, downClamp, upClamp);
        newRotation.x = Xrotation;
        newRotation.y += horizontalLookInput;
        transform.eulerAngles = newRotation;

    }
    private void Update()
    {
        Look();
    }
}
