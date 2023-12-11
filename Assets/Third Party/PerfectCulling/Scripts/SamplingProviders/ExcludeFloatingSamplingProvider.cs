// Perfect Culling (C) 2021 Patrick König
//

using UnityEngine;

namespace Koenigz.PerfectCulling.SamplingProviders
{
    [RequireComponent(typeof(PerfectCullingBakingBehaviour))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ExcludeFloatingSamplingProvider : SamplingProviderBase
    {
        [SerializeField] private LayerMask layerMask = ~0;
        [SerializeField] private float distance = 5f;

        public override string Name => nameof(ExcludeFloatingSamplingProvider) + $": Mask: {layerMask.value.ToString()}, Distance: {distance}"; 
      
        public override void InitializeSamplingProvider()
        {
        }

        public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
        {
            if (!Physics.Raycast(pos, -Vector3.up, distance, layerMask.value))
            {
                return false;
            }

            return true;
        }
    }
}