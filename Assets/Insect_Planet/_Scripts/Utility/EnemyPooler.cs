using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPooler : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    [SerializeField] private int poolSize = 20;
    private List<GameObject>[] _enemyPools;



    // Start is called before the first frame update
    void Start()
    {
        _enemyPools = new List<GameObject>[enemyPrefabs.Length];
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            _enemyPools[i] = new List<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                GameObject enemy = Instantiate(enemyPrefabs[i]);
                enemy.SetActive(false);
                _enemyPools[i].Add(enemy);
            }

        }

    }
    
    

    public GameObject GetEnemyFromPool(int enemyType)
    {
        for (int i = 0; i < _enemyPools[enemyType].Count; i++)
        {
            if (!_enemyPools[enemyType][i].activeInHierarchy)
            {

                return _enemyPools[enemyType][i];
            }
        }

        GameObject enemy = Instantiate(enemyPrefabs[enemyType]);
        _enemyPools[enemyType].Add(enemy);
        return enemy;
    }

    public void ReturnEnemyPool(GameObject enemy)
    {
        enemy.SetActive(false);
    }
}
