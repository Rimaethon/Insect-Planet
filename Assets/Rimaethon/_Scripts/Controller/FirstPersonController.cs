using Rimaethon._Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody _rb;
    #region Camera Movement Variables
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float fov = 60f;
    [SerializeField] private bool invertCamera;
    [SerializeField] private bool cameraCanMove = true;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 50f;
    CapsuleCollider _collider;
    private Vector2 _lookVector;
    [SerializeField] private bool lockCursor = true;
    private float _yaw;
    private float _pitch;
    private float _timer;
    #endregion
    [SerializeField] private Animator _animator;
    #region Movement Variables
    [SerializeField] private bool playerCanMove = true;
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] private float maxVelocityChange = 10f;
    private Vector2 _moveVector;
    [SerializeField] private bool _isWalking = false;

    #region Jump
    [SerializeField] bool enableJump = true;
    [SerializeField] float jumpPower = 5f;
    public bool _isGrounded;
    #endregion

    #region Crouch
    [SerializeField] private bool _enableCrouch = true;
    [SerializeField] private bool holdToCrouch;
    [SerializeField] private float speedReduction = .5f;
    private bool _isCrouched = false;
    private Vector3 _originalScale;
    #endregion

    #endregion

    #region Weapon Wobble

    [SerializeField] private Transform joint;
    [SerializeField] private Vector3 wobbleAmount = new Vector3(.01f, .025f, 0f);
    private Vector3 _jointOriginalPos;

    public FirstPersonController(bool invertCamera)
    {
        this.invertCamera = invertCamera;
    }

    #endregion


    #region OnEnable/OnDisable

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerMove, movementVector =>
        {
            _moveVector = movementVector;
        });
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerLook, lookVector =>
        {
            _lookVector = lookVector;
        });
        EventManager.Instance.AddHandler(GameEvents.OnPlayerJump, HandlePlayerJump);
        EventManager.Instance.AddHandler(GameEvents.OnPlayerCrouch, HandlePlayerCrouch);

    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerMove, movementVector =>
        {
            _moveVector = new Vector3(movementVector.x, 0, movementVector.y);
        });
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerLook, lookVector =>
        {
            _lookVector = new Vector3(lookVector.x, 0, lookVector.y);
        });
        EventManager.Instance.RemoveHandler(GameEvents.OnPlayerJump, HandlePlayerJump);
        EventManager.Instance.RemoveHandler(GameEvents.OnPlayerCrouch, HandlePlayerCrouch);

    }
    #endregion
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        playerCamera.fieldOfView = fov;
        _originalScale = transform.localScale;
        _jointOriginalPos = joint.localPosition;
        _collider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }


    private void Update()
    {
        CheckGround();
        HandlePlayerLook();
    }


    private void FixedUpdate()
    {
        #region Movement
        if (playerCanMove)
        {
            HandlePlayerMove();
        }
        #endregion
    }
    #region Jump

    private void HandlePlayerJump()
    {
        if(enableJump &&  _isGrounded)
        {
            Jump();
        }
    }

    #endregion

    #region Camera
    private void HandlePlayerLook()
    {
        if (!cameraCanMove) return;
        _yaw = playerCamera.gameObject.transform.localEulerAngles.y + _lookVector.x * mouseSensitivity;
        _pitch -= mouseSensitivity * _lookVector.y;
        _pitch = Mathf.Clamp(_pitch, -maxLookAngle, maxLookAngle);
        playerCamera.gameObject.transform.localEulerAngles = new Vector3(_pitch,_yaw, 0);
    }
    #endregion
    private void HandlePlayerMove()
    {
        Vector3 targetVelocity = new Vector3(_moveVector.x, 0, _moveVector.y);
        _isWalking = (targetVelocity.x != 0 || targetVelocity.z != 0) && _isGrounded;
        targetVelocity = playerCamera.gameObject.transform.TransformDirection(targetVelocity) * walkSpeed ;
        Vector3 velocity = _rb.velocity;
        Vector3 velocityChange = (targetVelocity- velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        _rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }
    private void Jump()
    {
        if (!_isGrounded) return;
        _rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
        _isGrounded = false;
    }
    private void HandlePlayerCrouch()
    {
        if(!_enableCrouch)
            return;
        if(_isCrouched)
        {
            walkSpeed /= speedReduction;
            _collider.height = 2f;
            _isCrouched = false;
        }
        else
        {
            walkSpeed *= speedReduction;
            _collider.height = 1f;
            _isCrouched = true;
        }

    }

}
