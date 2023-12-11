// Perfect Culling (C) 2022 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using Koenigz.PerfectCulling;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingBakerNativeVulkanWin64Handle : PerfectCullingBakerHandle
    {
        public int[] m_hash;

        private readonly System.Threading.EventWaitHandle m_waitHandle = new System.Threading.AutoResetEvent(false);

        private bool m_done = false;
        
        protected override void DoComplete()
        {
            // Nothing to do here other than waiting for it being populated.

            // We can avoid the overhead if we know we are already done.
            if (m_done)
            {
                return;
            }
            
            m_waitHandle.WaitOne();
            m_waitHandle.Reset();

            // TODO: Sanity test
        }

        public void MarkCompleted()
        {
            m_done = true;
            
            m_waitHandle.Set();
        }
    }
}