using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Rimaethon._Scripts.Utility
{
    public class RaycastToPlayer : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private LayerMask obstacleLayer;
        private int rayCount = 5;
        private Vector3 targetPoint;

        private void Awake()
        {
        }

        private void Update()
        {
            var maxDistance = Vector3.Distance(transform.position, player.position);

            for (var i = 0; i < rayCount; i++)
            {
                var targetPoint = GetTargetPointOnPlayer(i);
                var direction = (targetPoint - transform.position).normalized;

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
            var targetHeight = (float)index / (rayCount - 1);
            var playerLowestYBound = player.GetComponent<Collider>().bounds.min.y;
            var playerColliderYSize =
                player.GetComponent<Collider>().bounds.size
                    .y; // Change this to the size of the player's collider or bounding box if needed
            var playerPosition = player.position;
            playerPosition.y = playerLowestYBound;
            targetPoint = playerPosition + Vector3.up * (playerColliderYSize * targetHeight);


            return targetPoint;
        }
    }
}