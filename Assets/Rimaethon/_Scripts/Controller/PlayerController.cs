using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lookSpeed = 60f;
    public float jumpPower = 8f;
    public float gravity = 9.81f;

    public float jumpTimeLeniency = 0.1f;
    public Shooter playerShooter;
    public Health playerHealth;
    public List<GameObject> disableWhileDead;

    public CharacterController characterController;
    public InputManager inputManager;
    private bool doubleJumpAvaliable;

    private Vector3 moveDirection;
    private float timeToStopBeingLenient;


    private void Start()
    {
        if (playerHealth.currentHealth <= 0)
        {
            foreach (var inGameObject in disableWhileDead) inGameObject.SetActive(false);
            return;
        }

        foreach (var inGameObject in disableWhileDead) inGameObject.SetActive(true);

        SetUpCharacterController();
        SetUpInputManger();
    }


    private void Update()
    {
        ProcessMovement();
        ProcessRotation();
    }

    private void SetUpCharacterController()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            Debug.LogError(
                "The player controller script does not have a character controller on the same game object!");
    }

    private void SetUpInputManger()
    {
        inputManager = InputManager.instance;
    }

    private void ProcessMovement()
    {
        //Get the input from the input manager
        var leftRightInput = inputManager.horizontalMoveAxis;
        //Debug.Log("The left right input value is:"+leftRightInput);
        var forwardBackwardInput = inputManager.verticalMoveAxis;
        var jumpPressed = inputManager.jumpPressed;

        //Handle the control of the player while it is on the ground
        if (characterController.isGrounded)
        {
            doubleJumpAvaliable = true;
            timeToStopBeingLenient = Time.time + jumpTimeLeniency;
            //set the movement direction to be recieved input,set y to 0 since we are grounded
            moveDirection = new Vector3(leftRightInput, 0, forwardBackwardInput);

            //Set the move direction in relation to the transform instead of its global origin.
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection = moveDirection * moveSpeed;

            if (jumpPressed) moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection = new Vector3(leftRightInput * moveSpeed, moveDirection.y, forwardBackwardInput * moveSpeed);

            moveDirection = transform.TransformDirection(moveDirection);

            if (jumpPressed && Time.time < timeToStopBeingLenient) moveDirection.y = jumpPower;
            if (jumpPressed && doubleJumpAvaliable)
            {
                moveDirection.y = jumpPower;
                doubleJumpAvaliable = false;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        if (characterController.isGrounded && moveDirection.y < 0) moveDirection.y = -0.3f;


        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void ProcessRotation()
    {
        var horizontalLookInput = inputManager.horizontalLookAxis;
        var verticalLookInput = inputManager.verticalLookAxis;
        var playerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(playerRotation.x / 2 * Time.deltaTime,
            playerRotation.y + horizontalLookInput * lookSpeed * Time.deltaTime
            , playerRotation.z));
    }
}