using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public Transform player;
    public LayerMask obstacleLayer;
    

    private void Start()
    {
        bool isVisible = CheckVisibility();
        if (isVisible)
        {
        }
    }

    private bool CheckVisibility()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, obstacleLayer))
        {
            if (hit.collider.gameObject != player.gameObject)
            {
                return false; 
            }
        }
        
        return true; 
    }
}
