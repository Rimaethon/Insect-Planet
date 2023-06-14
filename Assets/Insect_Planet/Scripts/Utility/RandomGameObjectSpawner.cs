using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class was created to allow us to have any of the icicles on the icicle projectile at random
/// It creates a random prefab from a list of prefabs and makes the attached gameobect the parent of the newly created game object
/// </summary>
public class RandomGameObjectSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The gameobjects to spawn from randomly")]
    public List<GameObject> gameObjects;

    /// <summary>
    /// Standard Unity function called once before the firt Update call
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Start()
    {
        SpawnRandom();
    }

    /// <summary>
    /// Description:
    /// Randomly spawns one of the prefabs in the list and makes the gameobject this script is attached to
    /// its parent
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void SpawnRandom()
    {
        int randomIndex = Random.Range(0, gameObjects.Count);
        Instantiate(gameObjects[randomIndex], transform.position, transform.rotation, transform);
    }
}
