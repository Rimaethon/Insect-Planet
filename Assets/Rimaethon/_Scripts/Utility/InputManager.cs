using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    // A global instance for scripts to reference
    public static InputManager instance;

    /// <summary>
    /// Description:
    /// Standard Unity Function called when the script is loaded
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Awake()
    {
        // Set up the instance of this
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destructable.DoDestroy(this.gameObject);
        }
    }

    [Header("Player Movement Input")]
    [Tooltip("The horizontal movmeent input of the player.")]
    public float horizontalMoveAxis;
    [Tooltip("The vertical movement input of the player.")]
    public float verticalMoveAxis;

    /// <summary>
    /// Description:
    /// Reads and stores the movement input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the movement input</param>
    public void ReadMovementInput(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        horizontalMoveAxis = inputVector.x;
        verticalMoveAxis = inputVector.y;
    }

    [Header("Look Around input")]
    [Tooltip("The horizontal look input")]
    public float horizontalLookAxis;
    [Tooltip("The vertical look input")]
    public float verticalLookAxis;

    /// <summary>
    /// Description:
    /// Reads and stores the look input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the look input</param>
    public void ReadLookInput(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        horizontalLookAxis = inputVector.x;
        verticalLookAxis = inputVector.y;
    }

    [Header("Player Fire Input")]
    [Tooltip("Whether or not the fire button was pressed this frame")]
    public bool firePressed;
    [Tooltip("Whether or not the fire button is being held down")]
    public bool fireHeld;

    /// <summary>
    /// Description:
    /// Reads and stores the fire input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the fire input</param>
    public void ReadFireInput(InputAction.CallbackContext context)
    {
        firePressed = !context.canceled;
        fireHeld = !context.canceled;
        StartCoroutine("ResetFireStart");
    }

    /// <summary>
    /// Description
    /// Coroutine that resets the fire pressed variable after one frame
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Allows this to function as a coroutine</returns>
    private IEnumerator ResetFireStart()
    {
        yield return new WaitForEndOfFrame();
        firePressed = false;
    }

    [Header("Player Jump Input")]
    [Tooltip("Whether or not the jump button was pressed this fame")]
    public bool jumpPressed;
    [Tooltip("Whether or not the jump button is being held down")]
    public bool jumpHeld;

    /// <summary>
    /// Description:
    /// Reads and stores the jump input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the jump input</param>
    public void ReadJumpInput(InputAction.CallbackContext context)
    {
        jumpPressed = !context.canceled;
        jumpHeld = !context.canceled;
        StartCoroutine("ResetJumpStart");
    }

    /// <summary>
    /// Description
    /// Coroutine that resets the jump pressed variable after one frame
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Allows this to function as a coroutine</returns>
    private IEnumerator ResetJumpStart()
    {
        yield return new WaitForEndOfFrame();
        jumpPressed = false;
    }

    [Header("Pause Input")]
    [Tooltip("Whether or not the pause button was pressed this frame")]
    public bool pausePressed;

    /// <summary>
    /// Description:
    /// Reads and stores the pause input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the pause input</param>
    public void ReadPauseInput(InputAction.CallbackContext context)
    {
        pausePressed = !context.canceled;
        StartCoroutine(ResetPausePressed());
    }

    /// <summary>
    /// Description
    /// Coroutine that resets the paused pressed variable after one frame
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Allows this to function as a coroutine</returns>
    private IEnumerator ResetPausePressed()
    {
        yield return new WaitForEndOfFrame();
        pausePressed = false;
    }

    [Header("Cycle weapon input")]
    [Tooltip("The input from the axis that cycles weapons")]
    public float cycleWeaponInput;

    /// <summary>
    /// Description:
    /// Reads and stores the cycle weapon input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the cycle weapon input</param>
    public void ReadCycleWeaponInput(InputAction.CallbackContext context)
    {
        Vector2 mouseScrollInput = context.ReadValue<Vector2>();
        if (mouseScrollInput.y == 0)
        {
            cycleWeaponInput = 0;
        }
        else
        {
            cycleWeaponInput = Mathf.Sign(mouseScrollInput.y);
        }
    }

    [Header("Next weapon input")]
    [Tooltip("Whether or not the next weapon button was pressed this frame")]
    public bool nextWeaponPressed;

    /// <summary>
    /// Description:
    /// Reads and stores the next weapon input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the next weapon input</param>
    public void ReadNextWeaponInput(InputAction.CallbackContext context)
    {
        nextWeaponPressed = !context.canceled;
        StartCoroutine("ResetNextWeaponPressedStart");
    }

    /// <summary>
    /// Description
    /// Coroutine that resets the next weapon pressed variable after one frame
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Allows this to function as a coroutine</returns>
    private IEnumerator ResetNextWeaponPressedStart()
    {
        yield return new WaitForEndOfFrame();
        nextWeaponPressed = false;
    }

    [Header("Previous weapon input")]
    [Tooltip("Whether or not the previous weapon button was pressed this frame")]
    public bool previousWeaponPressed;

    /// <summary>
    /// Description:
    /// Reads and stores the previous weapon input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the previous weapon input</param>
    public void ReadPreviousWeaponInput(InputAction.CallbackContext context)
    {
        previousWeaponPressed = !context.canceled;
        StartCoroutine("ResetPreviousWeaponPressedStart");
    }

    /// <summary>
    /// Description
    /// Coroutine that resets the previous weapon pressed variable after one frame
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Allows this to function as a coroutine</returns>
    private IEnumerator ResetPreviousWeaponPressedStart()
    {
        yield return new WaitForEndOfFrame();
        previousWeaponPressed = false;
    }
}
