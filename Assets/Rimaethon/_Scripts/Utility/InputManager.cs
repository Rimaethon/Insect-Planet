using System;
using Rimaethon._Scripts.Utility;
using Rimaethon.Scripts.Managers;
using Rimaethon.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rimaethon.Runtime.Managers
{
    public class InputManager : PrivatePersistentSingleton<InputManager>,PlayerInputActionMaps.IPlayerActions
    {
        private bool _isPlayerDead;
        private float _movementDirection;
        private PlayerInputActionMaps _playerInputs;

   

        private void OnEnable()
        {
            EnableInputs();
            EventManager.Instance.AddHandler(GameEvents.OnPlayerDead, HandlePlayerDead);
            EventManager.Instance.AddHandler(GameEvents.OnPlayerRespawned, HandlePlayerRevive);
            EventManager.Instance.AddHandler(GameEvents.OnPause, DisableMovement);
            EventManager.Instance.AddHandler(GameEvents.OnResume, EnableInputs);
        }

        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler(GameEvents.OnPlayerDead, HandlePlayerDead);
            EventManager.Instance.RemoveHandler(GameEvents.OnPlayerRespawned, HandlePlayerRevive);
            EventManager.Instance.RemoveHandler(GameEvents.OnPause, DisableMovement);
            EventManager.Instance.RemoveHandler(GameEvents.OnResume, EnableInputs);
        }

        protected override void Awake()
        {
            _playerInputs = new PlayerInputActionMaps(); 
            base.Awake();
        }


        public void OnMovement(InputAction.CallbackContext context)
        {
            Vector2 movementVector = context.ReadValue<Vector2>();
            EventManager.Instance.Broadcast(GameEvents.OnPlayerMove, movementVector);
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            EventManager.Instance.Broadcast(GameEvents.OnPlayerClick);
        }

        public void OnCycleWeapon(InputAction.CallbackContext context)
        {
            if(context.ReadValue<float>()==0) return;
            int value = context.ReadValue<float>()>0?1:-1;
            EventManager.Instance.Broadcast(GameEvents.OnPlayerCycleWeapon, value);
        }

        public void OnChangeWeaponNum(InputAction.CallbackContext context)
        {
            EventManager.Instance.Broadcast(GameEvents.OnPlayerWeaponChange, context.ReadValue<float>());
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            
            EventManager.Instance.Broadcast(GameEvents.OnPlayerCrouch);

            if (context.canceled)
            { 
                EventManager.Instance.Broadcast(GameEvents.OnPlayerCrouch);
            }
            
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            EventManager.Instance.Broadcast(GameEvents.OnPlayerJump);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 movementVector = context.ReadValue<Vector2>().normalized;
            EventManager.Instance.Broadcast<Vector2>(GameEvents.OnPlayerMove, movementVector);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Vector2 lookVector = context.ReadValue<Vector2>();
            EventManager.Instance.Broadcast<Vector2>(GameEvents.OnPlayerLook, lookVector);
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            EventManager.Instance.Broadcast(GameEvents.OnUIBack);
        }

       


        private void HandlePlayerDead()
        {
            _isPlayerDead = true;
            DisableMovement();
        }

        private void HandlePlayerRevive()
        {
            _isPlayerDead = false;
            EnableInputs();
        }

        private void DisableMovement()
        {
            _playerInputs.Player.Movement.performed -= OnMovement;
            _playerInputs.Player.Jump.performed -= OnJump;
        }


        private void EnableInputs()
        {
            _playerInputs.Enable();
            Debug.Log("The player inputs are enabled");
            _playerInputs.Player.Movement.performed += OnMovement;
            _playerInputs.Player.Movement.canceled += OnMovement;
            _playerInputs.Player.ChangeWeaponNum.performed += OnChangeWeaponNum;
            _playerInputs.Player.Fire.performed += OnFire;
            _playerInputs.Player.Look.performed += OnLook;
            _playerInputs.Player.Look.canceled += OnLook;
            _playerInputs.Player.CycleWeapon.performed += OnCycleWeapon;
            _playerInputs.Player.Jump.performed += OnJump;  
            _playerInputs.Player.Pause.performed += OnPause;
            _playerInputs.Player.Crouch.performed += OnCrouch;
            _playerInputs.Player.Crouch.canceled += OnCrouch;
        }
    }

   
}