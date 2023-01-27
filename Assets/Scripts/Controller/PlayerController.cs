using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class handles the movement of the player with given input from the input manager
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed which player moves")]
    public float moveSpeed = 2f;
    [Tooltip("The speed which player rotates to look left and rate(in degrees")]
    public float lookSpeed = 60f;
    [Tooltip("The power which player jumps")]
    public float jumpPower = 8f;
    [Tooltip("The strength of gravity")]
    public float gravity = 9.81f;

    [Header("Jump Timing")]
    public float jumpTimeLeniency = 0.1f;
    float timeToStopBeingLenient=0f;
    [Header("Requried References")]
    [Tooltip("The player shooter script that fires projectiles")]
    public Shooter playerShooter;
    bool doubleJumpAvaliable;
    public Health playerHealth;
    public List<GameObject> disableWhileDead;

    //The character controller component on the player
    public CharacterController characterController;
    public InputManager inputManager;
    public static Transform localPlayer;

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Start()
    {
        localPlayer = transform;
        if(playerHealth.currentHealth<=0)
        {
            foreach(GameObject inGameObject in disableWhileDead)
            {
                inGameObject.SetActive(false);

            }
            return;
        }else
        {
            foreach (GameObject inGameObject in disableWhileDead)
            {
                inGameObject.SetActive(true);

            }
        }
                
        SetUpCharacterController();
        SetUpInputManger();

    }

    private void SetUpCharacterController()
    {
        characterController = GetComponent<CharacterController>(); 
        if(characterController == null)
        {
            Debug.LogError("The player controller script does not have a character controller on the same game object!");
        }
    }
    void SetUpInputManger()
    {
        inputManager = InputManager.instance;
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Update()
    {
        ProcessMovement();
        ProcessRotation();
    }

    Vector3 moveDirection;

    void ProcessMovement()
    {
        //Get the input from the input manager
        float leftRightInput = inputManager.horizontalMoveAxis;
        //Debug.Log("The left right input value is:"+leftRightInput);
        float forwardBackwardInput=inputManager.verticalMoveAxis;
        bool jumpPressed=inputManager.jumpPressed;

        //Handle the control of the player while it is on the ground
        if (characterController.isGrounded)
        {
            doubleJumpAvaliable=true;
            timeToStopBeingLenient = Time.time + jumpTimeLeniency;
            //set the movement direction to be recieved input,set y to 0 since we are grounded
            moveDirection = new Vector3(leftRightInput, 0, forwardBackwardInput);

            //Set the move direction in relation to the transform instead of its global origin.
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection=moveDirection*moveSpeed;

            if(jumpPressed)
            {
                moveDirection.y = jumpPower;
            }
        }
        else 
        {
        
            moveDirection=new Vector3(leftRightInput*moveSpeed, moveDirection.y, forwardBackwardInput*moveSpeed);

            moveDirection = transform.TransformDirection(moveDirection);

            if(jumpPressed&&Time.time<timeToStopBeingLenient)
            {
                moveDirection.y = jumpPower;
            }
            if(jumpPressed&&doubleJumpAvaliable)
            {
                moveDirection.y = jumpPower;
                doubleJumpAvaliable = false;
            }
        }
        moveDirection.y-=gravity*Time.deltaTime;

        if(characterController.isGrounded && moveDirection.y<0)
        {

            moveDirection.y = -0.3f;
        }


        characterController.Move(moveDirection*Time.deltaTime);
        
    }

    void ProcessRotation()
    {
        float horizontalLookInput = inputManager.horizontalLookAxis;
        float verticalLookInput = inputManager.verticalLookAxis;
        Vector3 playerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(playerRotation.x/2*Time.deltaTime, playerRotation.y+horizontalLookInput*lookSpeed*Time.deltaTime
            ,playerRotation.z));

    }

}
