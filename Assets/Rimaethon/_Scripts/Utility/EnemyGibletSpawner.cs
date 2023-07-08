using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This class handles the polish effect of exploding the enemies into giblets when they die
/// </summary>
public class EnemyGibletSpawner : MonoBehaviour
{
    [Header("Settings")] [Tooltip("The prefab giblets to spawn")]
    public List<GameObject> giblets;

    [Range(3, 10)] [Tooltip("The number of giblets to spawn")]
    public int numberToSpawn = 5;

    [Min(0)] [Tooltip("The minimum force to potentially apply to a spawned giblet")]
    public float minimumForce;

    [Min(0)] [Tooltip("The maximum force to potentially apply to a spawned giblet")]
    public float maximumForce = 30;


    /// <summary>
    ///     Description:
    ///     Standard Unity function that is called once before the first Update call
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void Start()
    {
        SpawnGiblets();
    }

    /// <summary>
    ///     Description:
    ///     Calls SpawnGiblet numberToSpawn times
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void SpawnGiblets()
    {
        for (var i = 0; i < numberToSpawn; i++) SpawnGiblet();
    }

    /// <summary>
    ///     Description:
    ///     Spawns a random giblet and gives it a random rotation, force and direction
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void SpawnGiblet()
    {
        // Get the giblet index to spawn
        var gibletIndex = Random.Range(0, giblets.Count);

        // Control for settings mistakes
        if (maximumForce < minimumForce)
        {
            maximumForce = minimumForce;
            Debug.LogWarning("Giblet spawning minimum force is greater than maximum force");
        }

        // Get a random force value to apply
        var force = Random.Range(minimumForce, maximumForce);

        // Get a random direction to apply the force in
        var randomDirection =
            new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        // Get a random rotation to apply to the giblet
        var randomRotation = Random.rotation;

        // Create the giblet and store it in a variable
        var createdGiblet = Instantiate(giblets[gibletIndex], transform.position, randomRotation, null);

        var gibletRigidbody = createdGiblet.GetComponent<Rigidbody>();
        if (gibletRigidbody == null)
        {
            gibletRigidbody = createdGiblet.AddComponent<Rigidbody>();
            gibletRigidbody.useGravity = true;
        }

        // Apply the force and its direction to the rigidbody
        gibletRigidbody.AddForce(force * randomDirection);
    }
}