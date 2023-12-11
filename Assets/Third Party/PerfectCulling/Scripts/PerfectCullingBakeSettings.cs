// Perfect Culling (C) 2021 Patrick König
//

using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingBakeSettings
    {
        public PerfectCullingBakeGroup[] Groups;

        public HashSet<Renderer> AdditionalOccluders;

        public List<SamplingLocation> SamplingLocations;
        public int ActiveSamplingPositionCount;
        
        public int Width;
        public int Height;

        public struct SamplingLocation
        {
            public readonly Vector3 Position;
            public readonly bool Active;

            public SamplingLocation(Vector3 position, bool active)
            {
                this.Position = position;
                this.Active = active;
            }
        }
    }
}