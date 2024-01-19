using UnityEngine;
using Rimaethon._Scripts.Utility;
using Rimaethon.Scripts.Managers;

public class SceneLikeCamera : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f; // Speed of camera movement
    [SerializeField] private float lookSpeed = 1f; // Speed of camera rotation

    private Vector3 _moveVector; // Vector for camera movement
    private Vector2 _lookVector; // Vector for camera rotation

    private void Awake()
    {
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        // Subscribe to player movement and look events
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerMove, HandlePlayerMove);
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerLook, HandlePlayerLook);
    }

    private void OnDisable()
    {
        // Unsubscribe from player movement and look events
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerMove, HandlePlayerMove);
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerLook, HandlePlayerLook);
    }

    // Handle player movement input
    private void HandlePlayerMove(Vector2 movementVector)
    {
        _moveVector = new Vector3(movementVector.x, 0, movementVector.y);
    }

    // Handle player look input
    private void HandlePlayerLook(Vector2 lookVector)
    {
        _lookVector = lookVector;
    }

    // Update camera rotation based on player look input
    private void Look()
    {
        float horizontalLookInput = _lookVector.x * lookSpeed * Time.deltaTime;
        float verticalLookInput = _lookVector.y * lookSpeed * Time.deltaTime;

        // Apply horizontal rotation around y-axis
        Quaternion horizontalRotation = Quaternion.AngleAxis(horizontalLookInput, Vector3.up);
        transform.rotation = horizontalRotation;

        // Apply vertical rotation around x-axis
        Quaternion verticalRotation = Quaternion.AngleAxis(-verticalLookInput, Vector3.right);
        transform.rotation = verticalRotation;
    }

    private void LateUpdate()
    {
        // Update camera position based on player movement input
        _moveVector = transform.TransformDirection(_moveVector) * (moveSpeed * Time.deltaTime);
        transform.position += _moveVector;

        // Update camera rotation
        Look();
    }
}