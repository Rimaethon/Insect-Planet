// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.Linq;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingSceneColor : IDisposable
    {
        public int[] Hash => m_hash;
        public Color32[] Colors => m_rendererColors;
        
        private PerfectCullingBakeGroup[] m_allGroups;
        private HashSet<Renderer> m_additionalOccluders;
        
        private Color32[] m_rendererColors;
        private int[] m_hash;
        
        // Shader properties
        private readonly int m_propIdColor = Shader.PropertyToID("_Color");
        private readonly int m_propIdCull = Shader.PropertyToID("_Cull");
        
        // Materials
        private readonly Material m_rendererMaterial;

        private List<UnityEngine.Object> m_disposeList = new List<Object>();
        private List<MeshRenderer> m_tempRenderers = new List<MeshRenderer>();

        public PerfectCullingSceneColor(PerfectCullingBakeGroup[] groups, HashSet<Renderer> additionalOccluders, bool setupRenderers = true)
        {
            m_allGroups = groups;
            m_additionalOccluders = additionalOccluders;
            
            m_hash = new int[256 * 256 * 256];

            m_rendererColors = new Color32[m_allGroups.Length];

            GenerateColors();

#if UNITY_EDITOR
            m_rendererMaterial = PerfectCullingResourcesLocator.Instance.UnlitTagMaterial;
#endif
            
            if (m_rendererMaterial == null)
            {
                Debug.LogError("Missing material");
                
                return;
            }

            m_rendererMaterial = Object.Instantiate(m_rendererMaterial);
            m_disposeList.Add(m_rendererMaterial);

#if UNITY_EDITOR_WIN
            m_rendererMaterial.enableInstancing = SystemInfo.supportsInstancing;
#else
            m_rendererMaterial.enableInstancing = false; // Noticed some weirdness on OSX/Metal. Might work better on other problems but lets be safe and disable it for this reason :-(
#endif
            
            // Check for Tags
            Dictionary<Mesh, Mesh> flippedMeshes = new Dictionary<Mesh, Mesh>();
            
            for (int groupIndex = 0; groupIndex < m_allGroups.Length; ++groupIndex)
            {
                foreach (Renderer renderer in m_allGroups[groupIndex].renderers)
                {
                    if (renderer == null)
                    {
                        Debug.LogError("Some renderers are null. References to renderers have to be stable to not cause issues.");
                        
                        continue;
                    }
                    
                    PerfectCullingRendererTag tag = renderer.GetComponent<PerfectCullingRendererTag>();

                    if (tag == null)
                    {
                        continue;
                    }

                    if (!tag.RenderDoubleSided)
                    {
                        continue;
                    }

                    GameObject go = new GameObject(renderer.name + "_Flipped");
                    
                    // Also makes sure that we take into account parent position, rotation and scale
                    go.transform.parent = renderer.transform;
                    go.transform.localScale = Vector3.one;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    
                    MeshFilter mf = renderer.GetComponent<MeshFilter>();

                    if (mf == null || mf.sharedMesh == null)
                    {
                        continue;
                    }

                    if (!flippedMeshes.TryGetValue(mf.sharedMesh, out Mesh mesh))
                    {
                        mesh = Object.Instantiate(mf.sharedMesh);
                        mesh.triangles = mesh.triangles.Reverse().ToArray();
                        
                        flippedMeshes.Add(mf.sharedMesh, mesh);
                    }

                    MeshRenderer newR = go.AddComponent<MeshRenderer>();
                      
                    newR.sharedMaterial = renderer.sharedMaterial;
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    
                    m_disposeList.Add(newR);
                    m_disposeList.Add(mesh);

                    // We are using the Groups during rendering and don't want to break anything while doing so.
                    // We only want to visualize this anyway in Play Mode.
                    if (Application.isPlaying)
                    {
                        PrepareRenderer(newR, m_rendererColors[groupIndex], new MaterialPropertyBlock());
                    }
                    else
                    {
                        m_allGroups[groupIndex].renderers = m_allGroups[groupIndex].renderers.Append(newR).ToArray();
                    }
                    
                    // Keep track of them so we can remove them from the bake group again
                    m_tempRenderers.Add(newR);

                    break;
                }
            }

            if (setupRenderers)
            {
                SetupRenderers();
            }
        }

        public void Dispose()
        {
            foreach (var obj in m_disposeList)
            {
                Object.DestroyImmediate(obj);
            }

            for (var index = 0; index < m_allGroups.Length; index++)
            {
                PerfectCullingBakeGroup group = m_allGroups[index];
                
                group.renderers = group.renderers.Except(m_tempRenderers).ToArray();
            }

            m_disposeList.Clear();
            m_disposeList = null;
            
            m_hash = null;
        }
        
        void GenerateColors()
        {
            Color32[] colorTableColors = PerfectCullingColorTable.Instance.Colors;

            for (int i = 0; i < m_rendererColors.Length; ++i)
            {
                byte r = (byte) colorTableColors[i].r;
                byte g = (byte) colorTableColors[i].g;
                byte b = (byte) colorTableColors[i].b;

#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    unchecked
                    {
                        // Just to make it more visually distinct in the Editor
                        r = (byte)(g + b);
                    }
                }
#endif
                
                int index = (b * 256 * 256) + (g * 256) + r;//r + 256 * (g + 256 * b);
                
                m_rendererColors[i] = new Color32(r, g, b, byte.MaxValue);
                m_hash[index] = i;
            }
        }
        
        void SetupRenderers()
        {
            // Setup renderers in this bake
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

            HashSet<Renderer> allRenderers = new HashSet<Renderer>();
            
            for (int groupIndex = 0; groupIndex < m_allGroups.Length; ++groupIndex)
            {
                m_allGroups[groupIndex].ForeachRenderer((renderer) =>
                {
                    allRenderers.Add(renderer);
                    
                    PrepareRenderer(renderer, m_rendererColors[groupIndex], propBlock);
                });
            }

            if (m_additionalOccluders == null)
            {
                return;
            }
            
            foreach (Renderer renderer in m_additionalOccluders)
            {
                if (allRenderers.Contains(renderer))
                {
                    // Already part of the bake
                    continue;
                }

                PrepareRenderer(renderer, PerfectCullingConstants.ClearColor, propBlock);
            }
        }

        void PrepareRenderer(Renderer renderer, Color col, MaterialPropertyBlock propBlock)
        {
            Material[] allMaterials = renderer.sharedMaterials;
                
            renderer.enabled = true;
                
            // Turn all this stuff off to hopefully speed up our mesh rendering
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.lightmapIndex = -1;
            renderer.realtimeLightmapIndex = -1;

            // We only want to set the layer for the actual bake. We do not want to change the layer otherwise because this layer might be culled by the user.
            if (!Application.isPlaying)
            {
                renderer.gameObject.layer = PerfectCullingConstants.CamBakeLayer;
            }

            EBakeRenderMode forcedBakeRenderMode = EBakeRenderMode.None;

            PerfectCullingRendererTag rendererTag = renderer.GetComponent<PerfectCullingRendererTag>();

            if (rendererTag != null)
            {
                forcedBakeRenderMode = rendererTag.ForcedBakeRenderMode;
            }

            for (int materialIndex = 0; materialIndex < allMaterials.Length; ++materialIndex)
            {
                bool isTransparent = PerfectCullingUtil.IsMaterialTransparent(allMaterials[materialIndex]);

                switch (forcedBakeRenderMode)
                {
                    case EBakeRenderMode.Opaque:
                        isTransparent = false;
                        break;
                    
                    case EBakeRenderMode.Transparent:
                        isTransparent = true;
                        break;
                }
                        
                allMaterials[materialIndex] = m_rendererMaterial;

                col.a = (isTransparent ? PerfectCullingUtil.TRANSPARENT_RENDER_COLOR : PerfectCullingUtil.OPAQUE_RENDER_COLOR);
                    
                renderer.name += $", Color: {col}";
                
                propBlock.SetColor(m_propIdColor, col);
                
                renderer.SetPropertyBlock(propBlock, materialIndex);
            }

            renderer.sharedMaterials = allMaterials;
        }
    }
}