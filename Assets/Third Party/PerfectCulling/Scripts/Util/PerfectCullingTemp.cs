// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    /// <summary>
    /// Collection of objects we only need temporarily and thus can be re-used.
    /// </summary>
    public static class PerfectCullingTemp
    {
        public static readonly List<ushort> ListUshort = new List<ushort>(PerfectCullingConstants.MaxRenderers);
        
        public static readonly List<int> ListInt = new List<int>(PerfectCullingConstants.MaxRenderers);
    }
}