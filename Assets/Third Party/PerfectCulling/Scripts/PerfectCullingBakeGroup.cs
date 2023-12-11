// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling
{
    [System.Serializable]
    public class PerfectCullingBakeGroup
    {
        public readonly struct RuntimeGroupContent
        {
            public readonly Renderer Renderer;
            public readonly ShadowCastingMode ShadowCastingMode;

            public RuntimeGroupContent(Renderer renderer, ShadowCastingMode shadowCastingMode)
            {
                Renderer = renderer;
                ShadowCastingMode = shadowCastingMode;
            }
        }
        
        public enum GroupType
        {
            Other,
            LOD,
            User
        }

        // Bake data
        public GroupType groupType;
        public Renderer[] renderers = System.Array.Empty<Renderer>();
        public UnityEngine.Behaviour[] unityBehaviours = System.Array.Empty<UnityEngine.Behaviour>();
        public int vertexCount;

        // Run-time
        // We are not using a List because this is iterated quiet frequently
        [System.NonSerialized] private int m_runtimeGroupDataSize;
        [System.NonSerialized] private RuntimeGroupContent[] m_runtimeGroupData = System.Array.Empty<RuntimeGroupContent>();

        public void Init()
        {
            m_runtimeGroupData = new RuntimeGroupContent[renderers.Length];

            foreach (Renderer r in renderers)
            {
                if (r == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"{nameof(PerfectCullingBakeGroup)} contains invalid renderer reference, it will be skipped and the renderer won't be culled.");
#endif
                    
                    continue;
                }
                
                PushRuntimeRenderer(r);
            }
            
            // I think it should be fine to free the memory but it is not worth the risk at the moment
#if !UNITY_EDITOR
            // Free this memory
            //renderers = null;
#endif
        }
        /// <summary>
        /// Checks whether Renderer exists in group. Please use PerfectCullingAPI instead.
        /// </summary>
        public bool ContainsRuntimeRenderer(Renderer r)
        {
            for (int i = 0; i < m_runtimeGroupDataSize; ++i)
            {
                if (r == m_runtimeGroupData[i].Renderer)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Adds a new renderer as run-time data. Please use PerfectCullingAPI instead.
        /// </summary>
        public void PushRuntimeRenderer(Renderer renderer)
        {
            PushRuntimeGroupContent(new RuntimeGroupContent(renderer, renderer.shadowCastingMode));
        }

        /// <summary>
        /// Removes run-time renderer. Please use PerfectCullingAPI instead.
        /// </summary>
        public bool PopRuntimeRenderer(Renderer renderer)
        {
            int index = -1;
            
            for (int i = 0; i < m_runtimeGroupDataSize; ++i)
            {
                if (m_runtimeGroupData[i].Renderer == renderer)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return false;
            }

            if (m_runtimeGroupDataSize >= 2)
            {
                // Swap the element we want to remove with the element at the end
                (m_runtimeGroupData[index], m_runtimeGroupData[m_runtimeGroupDataSize - 1]) = (m_runtimeGroupData[m_runtimeGroupDataSize - 1], m_runtimeGroupData[index]);
            }

            // Pop
            PopRuntimeGroupContent();
            
            return true;
        }
        
        /// <summary>
        /// Adds new run-time data. Please use PerfectCullingAPI instead.
        /// </summary>
        private void PushRuntimeGroupContent(RuntimeGroupContent groupContent)
        {
            if (m_runtimeGroupDataSize >= m_runtimeGroupData.Length)
            {
                // We just double the array size so we don't need to do this too often...
                System.Array.Resize(ref m_runtimeGroupData, Mathf.Max(1, m_runtimeGroupDataSize * 2));
            }

            m_runtimeGroupData[m_runtimeGroupDataSize] = groupContent;

            ++m_runtimeGroupDataSize;
        }

        /// <summary>
        /// Removes run-time data and removes it. Please use PerfectCullingAPI instead.
        /// </summary>
        private RuntimeGroupContent PopRuntimeGroupContent()
        {
            if (m_runtimeGroupDataSize <= 0)
            {
                return default;
            }

            --m_runtimeGroupDataSize;
            
            return m_runtimeGroupData[m_runtimeGroupDataSize];
        }
        
        /// <summary>
        /// Clears all run-time data. Please use PerfectCullingAPI instead.
        /// </summary>
        public void ClearRuntimeRenderers()
        {
            m_runtimeGroupDataSize = 0;
        }
        
        public void Toggle(bool isVisible, bool forceNullCheck = false)
        {
            for (int i = 0; i < m_runtimeGroupDataSize; ++i)
            {
                RuntimeGroupContent groupContent = m_runtimeGroupData[i];
                
                PerfectCullingUtil.ToggleRenderer(groupContent.Renderer, isVisible, forceNullCheck, groupContent.ShadowCastingMode);
            }

            int unityBehavioursCount = unityBehaviours.Length;
            
            for (int i = 0; i < unityBehavioursCount; ++i)
            {
                if (forceNullCheck && unityBehaviours[i] == null)
                {
                    continue;
                }
                
                unityBehaviours[i].enabled = isVisible;
            }
        }

        public void ForeachRenderer(System.Action<Renderer> actionForRenderer)
        {
            foreach (Renderer r in renderers)
            {
#if UNITY_EDITOR
                if (r == null)
                {
                    Debug.LogError("Invalid renderer in bakeGroup");
                                
                    continue;
                }
#endif
                actionForRenderer.Invoke(r);
            }
        }

        // This only works in Edit mode due to Static Batching combining meshes.
        public bool CollectMeshStats()
        {
            int totalVertexCount = 0;
            
            foreach (Renderer rend in renderers)
            {
                if (rend == null)
                {
                    Debug.LogWarning($"Detected missing renderer");
                    
                    return false;
                }
                
                MeshFilter mf = rend.GetComponent<MeshFilter>();

                if (mf == null)
                {
                    Debug.LogWarning($"Detected Renderer without MeshFilter: {rend.name}", rend.gameObject);
                    
                    continue;
                }
                
                if (mf.sharedMesh == null)
                {
                    Debug.LogWarning($"Detected Renderer that is missing Mesh: {rend.name}", rend.gameObject);
                    
                    continue;
                }

                totalVertexCount += mf.sharedMesh.vertexCount;
            }

            vertexCount = totalVertexCount;

            return true;
        }
        
        /// <summary>
        /// Returns number of run-time renderers. Please use PerfectCullingAPI instead.
        /// </summary>
        public int GetRuntimeRendererCount()
        {
            return m_runtimeGroupDataSize;
        }
    }
    
    public class PerfectCullingBakeGroupComparer : IEqualityComparer<PerfectCullingBakeGroup>
    {
        public bool Equals(PerfectCullingBakeGroup x, PerfectCullingBakeGroup y)
        {   
            if (x == null || y == null)
            {
                return false;
            }

            if (x.renderers.Length != y.renderers.Length)
            {
                return false;
            }

            if (x.groupType != y.groupType)
            {
                return false;
            }
            
            for (int i = 0; i < x.renderers.Length; ++i)
            {
                if (x.renderers[i] != y.renderers[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(PerfectCullingBakeGroup obj)
        {
            if (obj == null)
            {
                return 0;
            }
            
            int hash = 17;

            unchecked
            {
                hash = hash * 13 + (int) obj.groupType;

                for (int i = 0; i < obj.renderers.Length; ++i)
                {
                    hash = hash * 13 + obj.renderers[i].GetInstanceID();
                }
                
                for (int i = 0; i < obj.unityBehaviours.Length; ++i)
                {
                    hash = hash * 13 + obj.unityBehaviours[i].GetInstanceID();
                }
            }

            return hash;
        }
    }
}