// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingBakerUnityCpuHandle : PerfectCullingBakerHandle
    {
        private const int TotalColors = 256 * 256 * 256;
        
        public Color32[] Pixels;

        public int[] m_hash;
        
        private static readonly bool[] hashes = new bool[TotalColors];
        
        private static readonly List<ushort> tmpIndices = new List<ushort>();

        // We can re-use them because we can only call GetData, etc. on the main thread anyway.
        protected override void DoComplete()
        {
            System.Array.Clear(hashes, 0, TotalColors);
            
            tmpIndices.Clear();

            int count = Pixels.Length;
            
            for (int indexPixel = 0; indexPixel < count; ++indexPixel)
            {
                Color32 pixel = Pixels[indexPixel];
                    
                int index = (pixel.b * 256 * 256) + (pixel.g * 256) + pixel.r; //r + 256 * (g + 256 * b);

                if (index <= 0)
                {
                    continue;
                }
                
                if (hashes[index])
                {
                    continue;
                }

                hashes[index] = true;
                
                tmpIndices.Add((ushort) m_hash[index]);
            }
            
            tmpIndices.Sort();

            indices = new ushort[tmpIndices.Count];
            
            tmpIndices.CopyTo(indices);
        }
    }
}