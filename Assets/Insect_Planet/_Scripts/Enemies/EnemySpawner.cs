using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public GameObject prefab;

    
    public enum SpawnMethod { Fixed, Random, Controlled }

 
    public SpawnMethod spawnMethod = SpawnMethod.Fixed;
    public float spawnRate = 5.0f;
    public Vector3 spawnAreaSize = Vector3.zero;
    public bool showSpawnArea = true;
    private float nextSpawnTime = Mathf.NegativeInfinity;

  
    private void Update()
    {
        TestSpawn();
    }

   
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

  
    private void TestSpawn()
    {
        if (Time.timeSinceLevelLoad > nextSpawnTime)
        {
            Spawn();
        }
    }

   
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
            GameObject instance = Instantiate(prefab, spawnLocation, Quaternion.identity, null);
        }
    }

   
    public Vector3 GetSpawnLocation()
    {
        Vector3 result = Vector3.zero;
        result.x = transform.position.x + Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
        result.y = transform.position.y + Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f);
        result.z = transform.position.z + Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f);
        return result;
    }
}
