// Perfect Culling (C) 2021 Patrick König
//

using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class BakeInformation
    {
        public PerfectCullingBakingBehaviour BakingBehaviour;
        public HashSet<Renderer> AdditionalOccluders;
    }
}