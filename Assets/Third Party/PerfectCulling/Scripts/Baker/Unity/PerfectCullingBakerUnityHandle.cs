// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingBakerUnityHandle : PerfectCullingBakerHandle
    {
        public ComputeBuffer appendBuf;
        public ComputeBuffer countBuf;

        public int[] m_hash;

        // We can re-use them because we can only call GetData, etc. on the main thread anyway.
        private static readonly int[] m_out = new int[PerfectCullingConstants.MaxRenderers];
        private static readonly int[] m_counterOutput = new int[1] { 0 };

        protected override void DoComplete()
        {
            ComputeBuffer.CopyCount(appendBuf, countBuf, 0);
                
            countBuf.GetData(m_counterOutput);

            int appendBufCount = m_counterOutput[0];
            
            indices = new ushort[appendBufCount];

            if (appendBufCount > 0)
            {
                // Partial read
                appendBuf.GetData(m_out, 0, 0, appendBufCount);

                for (int i = 0; i < appendBufCount; ++i)
                {
                    int q = m_out[i];

                    int b = q / (256 * 256);
                    q -= (b * 256 * 256);
                    int g = q / 256;
                    int r = q % 256;

                    // The value returned might actually overflow so we cannot use q directly
                    int index = (b * 256 * 256) + (g * 256) + r; //r + 256 * (g + 256 * b);

                    indices[i] = (ushort) m_hash[index];
                }

                System.Array.Sort(indices);
            }

            appendBuf.Dispose();
            countBuf.Dispose();

            appendBuf = null;
            countBuf = null;
            
        }
    }
}