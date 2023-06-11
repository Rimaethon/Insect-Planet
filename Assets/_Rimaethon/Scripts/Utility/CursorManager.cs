using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Handles management of the cursor and its state
/// </summary>
public class CursorManager : MonoBehaviour
{
    // An enum that defines the cursor state, used when setting the cursor state to be different values
    public enum CursorState { FPS, Menu, FPSVisable};

    [Header("Settings")]
    [Tooltip("The state to start the cursor in in this scene")]
    public CursorState startState = CursorState.FPS;

    // An instance of this to be referenced by other scripts
    public static CursorManager instance;

    /// <summary>
    /// Description:
    /// Standard Unity function which is called when the script is loaded in
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            ChangeCursorMode(startState);
        }
        else
        {
            Destructable.DoDestroy(gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// Changes cursor mode to match the desired state
    /// Input:
    /// CursorState cursorState
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="cursorState">The state to change to</param>
    public void ChangeCursorMode(CursorState cursorState)
    {
        if (cursorState == CursorState.FPS)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (cursorState == CursorState.FPSVisable)
        {

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
        else if (cursorState == CursorState.Menu)
        {

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

    }
}
