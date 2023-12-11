// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingResourcesLocator : ScriptableObject
    {
        private static PerfectCullingResourcesLocator m_instance;

        public static PerfectCullingResourcesLocator Instance
        {
            get
            {
                if (m_instance == null)
                {
                    PerfectCullingResourcesLocator[] tmp = Resources.LoadAll<PerfectCullingResourcesLocator>(PerfectCullingConstants.ResourcesFolder);

                    if (tmp.Length == 0)
                    {
                        return null;
                    }
                    
                    m_instance = tmp[0];
                }

                return m_instance;
            }
        }
        
        [Header("Internally used references. Please do not modify!")]
        public ComputeShader PointExtractorComputeShader;
        public Material UnlitTagMaterial;
        public UnityEngine.Object NativeLib;
        public UnityEngine.Object NativeVulkanLib;

        public PerfectCullingSettings Settings;
        public PerfectCullingColorTable ColorTable;
    }
}