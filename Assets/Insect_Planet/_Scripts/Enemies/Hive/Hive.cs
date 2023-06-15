using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles the "hive" or the boss of the game
/// </summary>
public class Hive : Enemy
{

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void Start()
    {
        // When this script starts up, set it up
        Setup();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void LateUpdate()
    {
        // Every frame, determine the correct movement, rotation, actions, etc. and cause the enemy to act accordingly
        HandleMovement();
        HandleActions();
        HandleAnimation();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called right before the script is destroyed
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void OnDestroy()
    {
        GameManager.instance.LevelCleared();
    }
}
