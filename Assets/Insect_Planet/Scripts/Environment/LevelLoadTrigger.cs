using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class will load a level when the player enters its trigger
/// </summary>
public class LevelLoadTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string nameOfSceneToLoad;

    /// <summary>
    /// Description:
    /// Standard Unity function called when a collider enters a trigger on this script's gameobject
    /// Input:
    /// Collider other
    /// Return:
    /// void
    /// </summary>
    /// <param name="other">The collider that caused the function call</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            SceneManager.LoadScene(nameOfSceneToLoad);
        }
    }

}
