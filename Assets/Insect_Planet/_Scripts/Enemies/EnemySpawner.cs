using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles spawning enemies
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("The prefab to use to spawn an enemy")]
    public GameObject prefab = null;

    /// <summary>
    /// Enum to help with different kinds of spawners
    /// </summary>
    public enum SpawnMethod { Fixed, Random, Controlled }

    [Tooltip("The intervals at which enemies are spawned:\n" +
        "\tFixed: Enemies will spawn every n seconds where n is the spawn rate.\n" +
        "\tRandom: Enemies will spawn at semi-random intervals, at most equal to the spawn rate.\n" +
        "\tControlled: Enemies will spawn from this spawner only when something else calles the Spawn method")]
    public SpawnMethod spawnMethod = SpawnMethod.Fixed;
    [Tooltip("The maximum time between spawns")]
    public float spawnRate = 5.0f;
    [Tooltip("The size in each dimension of the area in which this spawner will spawn enemies.")]
    public Vector3 spawnAreaSize = Vector3.zero;
    [Tooltip("Whether or not to display the spawn area for this spawner")]
    public bool showSpawnArea = true;
    // The time at which the next enemy will be spawned
    private float nextSpawnTime = Mathf.NegativeInfinity;

    /// <summary>
    /// Description:
    /// Standard Unity function called one per frame
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void Update()
    {
        // Every frame, try to spawn
        TestSpawn();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function that draws gizmos in the editor
    /// Draws a red box where the spawn area is
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showSpawnArea)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, spawnAreaSize);
            Gizmos.color = new Color(1, 0, 0, 0.25f);
            Gizmos.DrawCube(transform.position, spawnAreaSize);
        }
    }

    /// <summary>
    /// Description:
    /// Tests whether it is time to spawn an enemy
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void TestSpawn()
    {
        if (Time.timeSinceLevelLoad > nextSpawnTime)
        {
            Spawn();
        }
    }

    /// <summary>
    /// Description:
    /// Spawns an enemy if the prefab exists, also updates the next spawn time
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public void Spawn()
    {
        if (prefab != null)
        {
            switch (spawnMethod)
            {
                case SpawnMethod.Fixed:
                    nextSpawnTime = Time.timeSinceLevelLoad + spawnRate;
                    break;
                case SpawnMethod.Random:
                    nextSpawnTime = Time.timeSinceLevelLoad + spawnRate * Random.value;
                    break;
                case SpawnMethod.Controlled:
                    nextSpawnTime = Mathf.Infinity;
                    break;
            }
            Vector3 spawnLocation = GetSpawnLocation();
            GameObject instance = GameObject.Instantiate(prefab, spawnLocation, Quaternion.identity, null);
        }
    }

    /// <summary>
    /// Description:
    /// Determines the location at which to spawn an enemy
    /// Input: 
    /// none
    /// Return: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The location at which to spawn the enemy prefab</returns>
    public Vector3 GetSpawnLocation()
    {
        Vector3 result = Vector3.zero;
        result.x = transform.position.x + Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
        result.y = transform.position.y + Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f);
        result.z = transform.position.z + Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f);
        return result;
    }
}
