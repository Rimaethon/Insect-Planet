// Perfect Culling (C) 2021 Patrick König
//

using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public abstract class SamplingProviderBase : MonoBehaviour, IActiveSamplingProvider
    { 
        private PerfectCullingBakingBehaviour m_behaviour;

        protected virtual void OnEnable()
        {
            if (m_behaviour == null)
            {
                m_behaviour = GetComponent<PerfectCullingBakingBehaviour>();
            }

            // Add the sampling provider to the BakingBehaviour
            m_behaviour.AddSamplingProvider(this);
        }

        protected virtual void OnDisable()
        { 
            if (m_behaviour == null)
            {
                m_behaviour = GetComponent<PerfectCullingBakingBehaviour>();
            }
            
            // Remove the sampling provider to the BakingBehaviour
            m_behaviour.RemoveSamplingProvider(this);
        }

        public abstract string Name { get; }

        public abstract void InitializeSamplingProvider();

        public abstract bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos);
    }
}