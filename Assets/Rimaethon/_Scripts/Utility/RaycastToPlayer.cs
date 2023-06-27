using UnityEngine;

public class RaycastToPlayer : MonoBehaviour
{
    public Transform player;
    public LayerMask obstacleLayer;
    public int rayCount = 5;
    Vector3 targetPoint;

    private void Update()
    {
        float maxDistance = Vector3.Distance(transform.position, player.position);

        for (int i = 0; i < rayCount; i++)
        {
            Vector3 targetPoint = GetTargetPointOnPlayer(i);
            Vector3 direction = (targetPoint - transform.position).normalized;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, maxDistance, obstacleLayer))
            {
                
              
                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
            else
            {
                Debug.Log("I hit the player!");
                Debug.DrawRay(transform.position, direction * maxDistance, Color.green);
                
            }
        }
    }

    private Vector3 GetTargetPointOnPlayer(int index)
    {
        // Adjust the calculation based on the specific parts of the player you want to target
        float targetHeight = (float)index / (rayCount - 1);
        float playerLowestYBound = player.GetComponent<Collider>().bounds.min.y;
        float playerColliderYSize = player.GetComponent<Collider>().bounds.size.y;// Change this to the size of the player's collider or bounding box if needed
       var playerPosition = player.position;
       playerPosition.y = playerLowestYBound;
       targetPoint = playerPosition+Vector3.up * (playerColliderYSize* targetHeight);
       
      
    
        return targetPoint;
    }
}