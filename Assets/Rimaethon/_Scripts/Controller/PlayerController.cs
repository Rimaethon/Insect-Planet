using System;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using Rimaethon.Runtime.Managers;
using Rimaethon.Scripts.Managers;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpPower = 8f;
    public float gravity = 9.81f;

    public WeaponController playerWeaponController;
    [SerializeField] private float lerpTime=0.3f;
    private CharacterController _characterController;

    private Vector3 _moveDirection;
    private float _timeToStopBeingLenient;
    [SerializeField]private float targetMoveSpeed = 500f;
    [SerializeField] private float moveSpeed = 1f;
    private Vector3 _moveVector;
    private Vector3 _localMoveDirection;
    private Transform _cameraTransform;

    public PlayerController(CharacterController characterController)
    {
        this._characterController = characterController;
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _characterController = GetComponent<CharacterController>();
        if (Camera.main != null) _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerMove, HandlePlayerMove);
        EventManager.Instance.AddHandler(GameEvents.OnPlayerJump, HandlePlayerJump);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerMove, HandlePlayerMove);
        EventManager.Instance.RemoveHandler(GameEvents.OnPlayerJump, HandlePlayerJump);
    }
    private void Update()
    {
        float ypos = _moveDirection.y;

        _moveDirection = _cameraTransform.rotation * _localMoveDirection;
        _moveDirection.y = ypos;
       if(!IsGrounded())
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }

        moveSpeed = Mathf.Lerp(moveSpeed, targetMoveSpeed, lerpTime* Time.deltaTime)*_localMoveDirection.x;

        _characterController.Move(_moveDirection * (moveSpeed * Time.deltaTime));
    }
    private bool IsGrounded()
    {
        return _characterController.isGrounded;
    }

    private void HandlePlayerJump()
    {
        if (IsGrounded())
        {
            _moveDirection.y = jumpPower;
        }
    }


    private void HandlePlayerMove(Vector2 movementVector)
    {
        _localMoveDirection = new Vector3(movementVector.x, 0, movementVector.y);
    }


}
