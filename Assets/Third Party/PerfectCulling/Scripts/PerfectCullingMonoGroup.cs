// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public abstract class PerfectCullingMonoGroup : MonoBehaviour
    {
        [Header("Allows to exclude this from other PerfectCullingBakingBehaviours")]
        [SerializeField] public PerfectCullingBakingBehaviour[] restrictToBehaviours = System.Array.Empty<PerfectCullingBakingBehaviour>();
        
        public virtual List<Renderer> Renderers => throw new System.NotImplementedException();
        public virtual List<UnityEngine.Behaviour> UnityBehaviours => throw new System.NotImplementedException();

        public abstract void PreSceneSave(PerfectCullingBakingBehaviour bakingBehaviour);
        public abstract void PreBake(PerfectCullingBakingBehaviour bakingBehaviour);
        public abstract void PostBake(PerfectCullingBakingBehaviour bakingBehaviour);
    }
}