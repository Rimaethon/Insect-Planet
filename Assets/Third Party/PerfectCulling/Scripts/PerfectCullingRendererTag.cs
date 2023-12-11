// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Koenigz.PerfectCulling
{
    [RequireComponent(typeof(Renderer))]
    public class PerfectCullingRendererTag : MonoBehaviour
    {
        public bool ExcludeRendererFromBake => excludeRendererFromBake;
        public bool RenderDoubleSided => renderDoubleSided;
        public EBakeRenderMode ForcedBakeRenderMode => forcedBakeRenderMode;
        
        [SerializeField] private bool excludeRendererFromBake = false;
        [SerializeField] private bool renderDoubleSided = false;

        [SerializeField] private EBakeRenderMode forcedBakeRenderMode = EBakeRenderMode.None;
    }
}