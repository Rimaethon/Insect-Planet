// Perfect Culling (C) 2021 Patrick König
//

using UnityEngine;
using UnityEngine.AI;

namespace Koenigz.PerfectCulling.SamplingProviders
{ 
    [RequireComponent(typeof(PerfectCullingBakingBehaviour))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ExcludeTooFarFromNavMeshSamplingProvider : SamplingProviderBase
    {
        [SerializeField] private float distance = 2.5f;

        [Header("Maximum allowance on XZ plane (makes sure only straight up hits are allowed)")]
        [SerializeField] [UnityEngine.Range(0f, 1f)] private float maxDistanceX = 0.25f;
        [SerializeField] [UnityEngine.Range(0f, 1f)] private float maxDistanceZ = 0.25f;

        [Header("Allows to exclude cells below NavMesh (assumes Y is up)")] [SerializeField] private bool excludeBelowNavMesh = true;
        
        public override string Name => nameof(ExcludeTooFarFromNavMeshSamplingProvider);
      
        public override void InitializeSamplingProvider()
        {
        }

        public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
        {
            // Just check whether navmesh is within specified distance.
            if (!UnityEngine.AI.NavMesh.SamplePosition(pos, out NavMeshHit navMeshHit, distance, NavMesh.AllAreas))
            {
                return false;
            }

            Vector3 hitDir = (pos - navMeshHit.position).normalized;

            if (excludeBelowNavMesh && hitDir.y < float.Epsilon)
            {
                return false;
            }
            
            if (Mathf.Abs(navMeshHit.position.x - pos.x) > maxDistanceX)
            {
                return false;
            }
            
            if (Mathf.Abs(navMeshHit.position.z - pos.z) > maxDistanceZ)
            {
                return false;
            }
            
            return true;
        }
    }
}