using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveAttack : EnemyAttacker
{
    [Header("References")]
    [Tooltip("Spawners used in the attack")]
    public List<EnemySpawner> spawners;
    [Header("Settings")]
    [Min(1)]
    public int minimumToSpawn = 1;
    [Min(1)]
    public int maximimumToSpawn = 5;

    /// <summary>
    /// Description:
    /// Coroutine which causes this script to attack
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Coroutine that performs the attack</returns>
    protected override IEnumerator PerformAttack()
    {
        OnAttackStart();
        float t = 0;
        while (t < attackDuration)
        {
            yield return null;
            t += Time.deltaTime;
        }
        SpawnEnemies();
        OnAttackEnd();
    }

    /// <summary>
    /// Description:
    /// Spawns enemies
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void SpawnEnemies()
    {
        // Double check for incorrect settings
        if (minimumToSpawn > maximimumToSpawn)
        {
            maximimumToSpawn = minimumToSpawn + 1;
        }
        int spawnThisMany = Random.Range(minimumToSpawn, maximimumToSpawn);
        // Randomly spawn enemies
        for (int i=0; i < spawnThisMany; i++)
        {
            int spawnerIndex = Random.Range(0, spawners.Count);
            spawners[spawnerIndex].Spawn();
        }
    }
}
