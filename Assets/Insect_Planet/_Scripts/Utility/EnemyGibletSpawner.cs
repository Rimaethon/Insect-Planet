﻿using System.Collections.Generic;
using UnityEngine;


public class EnemyGibletSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The prefab giblets to spawn")]
    public List<GameObject> giblets;
    [Range(3, 10)]
    [Tooltip("The number of giblets to spawn")]
    public int numberToSpawn = 5;
    [Min(0)]
    [Tooltip("The minimum force to potentially apply to a spawned giblet")]
    public float minimumForce = 0;
    [Min(0)]
    [Tooltip("The maximum force to potentially apply to a spawned giblet")]
    public float maximumForce = 30;


  
    void Start()
    {
        SpawnGiblets();
    }

 
    void SpawnGiblets()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnGiblet();
        }
    }

  
    void SpawnGiblet()
    {
        // Get the giblet index to spawn
        int gibletIndex = Random.Range(0, giblets.Count);

        // Control for settings mistakes
        if (maximumForce < minimumForce)
        {
            maximumForce = minimumForce;
            Debug.LogWarning("Giblet spawning minimum force is greater than maximum force");
        }

        // Get a random force value to apply
        float force = Random.Range(minimumForce, maximumForce);

        // Get a random direction to apply the force in
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        // Get a random rotation to apply to the giblet
        Quaternion randomRotation = Random.rotation;

        // Create the giblet and store it in a variable
        GameObject createdGiblet = Instantiate(giblets[gibletIndex], transform.position, randomRotation, null);

        Rigidbody gibletRigidbody = createdGiblet.GetComponent<Rigidbody>();
        if (gibletRigidbody == null)
        {
            gibletRigidbody = createdGiblet.AddComponent<Rigidbody>();
            gibletRigidbody.useGravity = true;
        }

        // Apply the force and its direction to the rigidbody
        gibletRigidbody.AddForce(force * randomDirection);

    }
}
