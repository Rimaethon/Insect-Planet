// Perfect Culling (C) 2021 Patrick König
//

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Koenigz.PerfectCulling.SamplingProviders
{ 
    [RequireComponent(typeof(PerfectCullingBakingBehaviour))]
    [ExecuteAlways]
    public class ExcludeBelowColliderArraySamplingProvider : SamplingProviderBase
    {
        private static readonly Collider[] OverlapCollidersNonAllocBuffer = new Collider[128];
        
        [Header("Exclude cells below colliders")]
        [SerializeField] private Collider[] excludeBelow;

        [Header("Offset")] [SerializeField] [Range(-10f, 10f)] private float offsetY = 1.0f;
        [Header("Exclude cells inside colliders")] [SerializeField] private bool excludeInsideColliders = true;
        
        public override string Name => nameof(ExcludeBelowColliderArraySamplingProvider) + ": " + (excludeBelow != null ? excludeBelow.Length.ToString() : "(null)");
      
        public override void InitializeSamplingProvider()
        {
        }

        public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
        {
            if (excludeBelow == null)
            {
                return true;
            }
            
            bool cachedQueryHitBackfaces = Physics.queriesHitBackfaces;

            try
            {
                // MeshCollider might not have back faces but we need them to detect a hit.
                // That's why we temporarily override this.
                Physics.queriesHitBackfaces = true;

                RaycastHit raycastHit;

                // Exclude everything below the Custom_Exclude object.
                // To determine that we are below the object we simply raycast upwards.
                // We offset the ray starting position by 1 meter to also catch sampling positions that are inside the collision volume.

                Vector3 offset = new Vector3(0f, offsetY, 0f);

                foreach (Collider col in excludeBelow)
                {
                    if (col == null)
                    {
                        continue;
                    }
                    
                    if (col.Raycast(new Ray(pos - offset, Vector3.up), out raycastHit, float.MaxValue))
                    {
                        return false;
                    }

                    if (excludeInsideColliders)
                    {
                        int overlapCount = Physics.OverlapSphereNonAlloc(pos, 0.01f, OverlapCollidersNonAllocBuffer);

                        if (overlapCount > 0)
                        {
                            for (int i = 0; i < overlapCount; ++i)
                            {
                                if (OverlapCollidersNonAllocBuffer[i] == col)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                // Restore cached settings
                Physics.queriesHitBackfaces = cachedQueryHitBackfaces;
            }

            return true;
        }

        public void Migrate(float kvKey, HashSet<Collider> kvValue)
        {
            offsetY = kvKey;
            excludeBelow = kvValue.ToArray();
        }
    }
}