using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPoolSpawner : MonoBehaviour
{

    
    public EnemyPooler enemyPooler;

    public float spawnRadius = 20f;

    public float spawnInterval = 5f;

    private float spawnTimer = 0f;

    private Transform player;
    

    // Start is called before the first frame update
    void Start()
    {

        player = PlayerController.localPlayer;

    }

    // Update is called once per frame
    void Update()
    {
   
        spawnTimer += Time.deltaTime;
        if (spawnTimer>=spawnInterval)
        {
            spawnTimer = 0f;

            if (Vector3.Distance(transform.position, player.position) <= spawnRadius)
            {
                int enemyType = Random.Range(0, enemyPooler.enemyPrefabs.Length);
                GameObject enemy = enemyPooler.GetEnemyFromPool(enemyType);
                enemy.transform.position = transform.position;
                enemy.SetActive(true);
            }

        }
    }
}
