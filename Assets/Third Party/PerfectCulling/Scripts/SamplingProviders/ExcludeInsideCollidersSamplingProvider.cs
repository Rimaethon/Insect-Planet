// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using Koenigz.PerfectCulling;
using UnityEngine;

namespace Koenigz.PerfectCulling.SamplingProviders
{
    [RequireComponent(typeof(PerfectCullingBakingBehaviour))]
    [ExecuteAlways]
    public class ExcludeInsideCollidersSamplingProvider : SamplingProviderBase
    {
        private static readonly Collider[] OverlapCollidersNonAllocBuffer = new Collider[128];

        public override string Name => nameof(ExcludeInsideCollidersSamplingProvider);

        [Header("Notice: Concave MeshColliders are unsupported by PhysX and thus ignored")]
        public LayerMask layerMask = ~0;
        
        public override void InitializeSamplingProvider()
        {
        }
        
        public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
        {
            int overlapCount = Physics.OverlapSphereNonAlloc(pos, 0.01f, OverlapCollidersNonAllocBuffer, layerMask.value);

            return overlapCount <= 0;
        }
    }
}