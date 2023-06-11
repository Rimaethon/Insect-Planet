using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class which spawns an effect gameobject when destroyed
/// </summary>
public class OnDestroyEffectCreator : MonoBehaviour
{
    [Tooltip("The effect to create when destroyed.")]
    public GameObject destroyEffect = null;
    // Flag which tells whether the application is shutting down (avoids errors)
    public static bool quitting = false;

    /// <summary>
    /// Description:
    /// Ensures that the quitting flag gets set correctly to avoid instantiating prefabs as the application quits
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void OnApplicationQuit()
    {
        quitting = true;
    }

    /// <summary>
    /// Description:
    /// Standard Unity function Called when this gameobject is destroyed
    /// Input: 
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void OnDestroy()
    {
        //if (Application.isPlaying && !quitting)
        //{
        //    //CreateDestroyEffect();
        //}
    }

    /// <summary>
    /// Description:
    /// Instantiates the destruction effect prefab at this gameobject's position
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void CreateDestroyEffect()
    {
        if (destroyEffect != null)
        {
            GameObject obj = Instantiate(destroyEffect, transform.position, transform.rotation, null);
        }
    }
}
