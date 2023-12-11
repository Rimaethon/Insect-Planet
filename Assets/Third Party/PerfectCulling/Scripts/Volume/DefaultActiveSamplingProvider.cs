// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using Koenigz.PerfectCulling;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class DefaultActiveSamplingProvider : IActiveSamplingProvider
    {
        public static string DefaultActiveSamplingProviderName =>  nameof(DefaultActiveSamplingProvider);
        
        public string Name => DefaultActiveSamplingProviderName;

        private PerfectCullingExcludeVolume[] m_excludeVolumes;
        public PerfectCullingAlwaysIncludeVolume[] AlwaysIncludeVolumes;

        public void InitializeSamplingProvider()
        {
            m_excludeVolumes = Object.FindObjectsOfType<PerfectCullingExcludeVolume>();
            AlwaysIncludeVolumes = Object.FindObjectsOfType<PerfectCullingAlwaysIncludeVolume>();
        }
        
        public bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
        {
            foreach (var bound in m_excludeVolumes)
            {
                if (bound.IsPositionActive(bakingBehaviour, pos))
                {
                    return false;
                }
            }

            return true;
        }
    }
}