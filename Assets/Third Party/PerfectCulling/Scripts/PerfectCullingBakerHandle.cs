// Perfect Culling (C) 2021 Patrick König
//

using System;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    /// <summary>
    /// Handle returned after sampling a position.
    /// This allows to defer reading data from the GPU (AsyncGPUReadback doesn't work well in the Editor).
    /// 
    /// WARNING:
    /// Not calling Complete will result in memory leaks.
    /// </summary>
    public abstract class PerfectCullingBakerHandle
    {
        public ushort[] indices;

        public void Complete()
        {
            DoComplete();
            
            // Sanity check for duplicates. It's impossible for the GPU to return duplicates unless something got corrupted.
            for (int i = 1; i < indices.Length; ++i)
            {
                if (indices[i - 1] == indices[i])
                {
                    throw new System.Exception($"GPU returned duplicates. This should never happen. Please re-bake and try to restart Unity if this keeps on happening please file a bug report. Values: {indices[i - 1]} and {indices[i]}, Indices: {i - 1} and {i}");
                }
            }
        }
        
        protected abstract void DoComplete();
    }
}