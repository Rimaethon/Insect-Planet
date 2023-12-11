// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public abstract class PerfectCullingBaker : System.IDisposable
    {
        public virtual int BatchCount => PerfectCullingConstants.SampleBatchCount;
        
        protected readonly PerfectCullingBakeSettings m_bakeSettings;
        
        public PerfectCullingBaker(PerfectCullingBakeSettings perfectCullingBakeSettings)
        {
            m_bakeSettings = perfectCullingBakeSettings;
        }

        public abstract PerfectCullingBakerHandle SamplePosition(Vector3 pos);

        public abstract void Dispose();
    }
}