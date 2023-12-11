// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingBakerNativeWin64 : PerfectCullingBaker
    {
        // We do not really batch in Unity. So we can have a lower batch count and make the Editor UI more responsive.
        public override int BatchCount => 128;

        private const int StatusHello = 12;
        
        private static readonly string PipeName = "perfect-culling-renderer-pipe";
        
        private readonly PerfectCullingSceneColor m_sceneColor;

        private readonly NamedPipeServerStream m_namedPipeServer;
        
        private readonly BinaryReader m_pipeStreamReader;
        private readonly BinaryWriter m_pipeStreamWriter;

        private List<UnityEngine.Object> m_disposeList = new List<Object>();

        public static bool IsAvailable() => PerfectCullingResourcesLocator.Instance.NativeLib != null;
        
        public PerfectCullingBakerNativeWin64(PerfectCullingBakeSettings perfectCullingBakeSettings) 
            : base(perfectCullingBakeSettings)
        {
            m_sceneColor = new PerfectCullingSceneColor(perfectCullingBakeSettings.Groups, perfectCullingBakeSettings.AdditionalOccluders, false);
            
            List<Renderer> allRenderers = new List<Renderer>();
            Dictionary<Renderer, int> groupMapping = new Dictionary<Renderer, int>();

            const int AdditionalOccluderGroup = -1;
            
            for (int indexGroup = 0; indexGroup < m_bakeSettings.Groups.Length; ++indexGroup)
            {
                foreach (Renderer renderer in m_bakeSettings.Groups[indexGroup].renderers)
                {
                    if (groupMapping.TryGetValue(renderer, out int existingRendererGroup))
                    {
                        if (indexGroup != existingRendererGroup)
                        {
                            Debug.LogError(
                                $"Duplicated renderer {renderer.name} (group {indexGroup}, current {existingRendererGroup}), ignoring");
                        }
                        
                        continue;
                    }
                    
                    allRenderers.Add(renderer);
                    groupMapping.Add(renderer, indexGroup);
                }
            }

            if (m_bakeSettings.AdditionalOccluders != null)
            {
                allRenderers.AddRange(m_bakeSettings.AdditionalOccluders);

                foreach (Renderer r in m_bakeSettings.AdditionalOccluders)
                {
                    if (groupMapping.ContainsKey(r))
                    {
                        // Already part of the bake
                        continue;
                    }
                    
                    groupMapping.Add(r, AdditionalOccluderGroup);
                }
            }

            // Remove other LOD levels
            {
                LODGroup[] allLODs = Object.FindObjectsOfType<LODGroup>();

                HashSet<Renderer> allRenderersHashSet = new HashSet<Renderer>(allRenderers);
                HashSet<Renderer> lodKeepHashSet = new HashSet<Renderer>();
                HashSet<Renderer> lodOtherHashSet = new HashSet<Renderer>();

                foreach (LODGroup lodGroup in allLODs)
                {
                    LOD[] lods = lodGroup.GetLODs();
                    
                    // We simply keep the first LOD that contains Renderers
                    int lodToKeep = 0;

                    for (int i = 0; i < lods.Length; ++i)
                    {
                        if (lods[i].renderers.Length != 0)
                        {
                            lodToKeep = i;
                            
                            break;
                        }
                    }

                    for (int i = 0; i < lods.Length; ++i)
                    {
                        foreach (Renderer r in lods[i].renderers)
                        {
                            if (i == lodToKeep)
                            {
                                lodKeepHashSet.Add(r);
                            }
                            else
                            {
                                lodOtherHashSet.Add(r);
                            }
                        }
                    }
                }
                
                foreach (Renderer r in lodOtherHashSet)
                {
                    if (lodKeepHashSet.Contains(r))
                    {
                        continue;
                    }

                    if (lodOtherHashSet.Contains(r) && allRenderersHashSet.Contains(r))
                    {
                        allRenderers.Remove(r);
                    }
                }
            }

            // Split
            {
                List<MeshFilter> meshFilters = new List<MeshFilter>();

                foreach (Renderer renderer in allRenderers)
                {
                    MeshFilter mf = renderer.GetComponent<MeshFilter>();

                    if (mf == null || mf.sharedMesh == null)
                    {
                        continue;
                    }
                    
                    meshFilters.Add(mf);
                }

                List<CombineInstance> combine = new List<CombineInstance>();
                
                for (int indexCombineInstance = 0; indexCombineInstance < meshFilters.Count; ++indexCombineInstance)
                {
                    Renderer renderer = meshFilters[indexCombineInstance].GetComponent<Renderer>();

                    Mesh oldMesh = meshFilters[indexCombineInstance].sharedMesh;
                    Material[] mats = renderer.sharedMaterials;
                    
                    Mesh colorMesh = GameObject.Instantiate(oldMesh);

                    m_disposeList.Add(colorMesh);

                    Color32[] cols = new Color32[colorMesh.vertexCount];

                    // Might have more materials or sub-meshes. Need to cap it to the lowest number.
                    int maxMaterialCount = Mathf.Min(mats.Length, colorMesh.subMeshCount);

                    if (mats.Length != colorMesh.subMeshCount)
                    {
                        Debug.LogWarning($"Number of materials ({mats.Length}) doesn't match number of sub-meshes ({colorMesh.subMeshCount}) for {renderer.name}.", renderer.gameObject);
                    }
                    
                    for (int indexMaterial = 0; indexMaterial < maxMaterialCount; ++indexMaterial)
                    {
                        int[] tris = colorMesh.GetTriangles(indexMaterial);
                        
                        // TODO: This is always false because its already changed to the other material
                        bool isTransparent = PerfectCullingUtil.IsMaterialTransparent(mats[indexMaterial]);

                        Color32 actualColor = PerfectCullingConstants.ClearColor;
                        
                        if (groupMapping[renderer] != AdditionalOccluderGroup)
                        {
                            actualColor = m_sceneColor.Colors[groupMapping[renderer]];
                        }
                        actualColor.a = (isTransparent ? PerfectCullingUtil.TRANSPARENT_RENDER_COLOR : PerfectCullingUtil.OPAQUE_RENDER_COLOR);

                        PerfectCullingRendererTag tag = renderer.GetComponent<PerfectCullingRendererTag>();

                        if (tag != null)
                        {
                            switch (tag.ForcedBakeRenderMode)
                            {
                                case EBakeRenderMode.Opaque:
                                    actualColor.a = PerfectCullingUtil.OPAQUE_RENDER_COLOR;
                                    break;
                                
                                case EBakeRenderMode.Transparent:
                                    actualColor.a = PerfectCullingUtil.TRANSPARENT_RENDER_COLOR;
                                    break;
                            }
                        }
                        
                        for (int indexTri = 0; indexTri < tris.Length; ++indexTri)
                        {
                            cols[tris[indexTri]] = actualColor;
                        }
                    }
                    
                    colorMesh.colors32 = cols;
                    
                    for (int indexMaterial = 0; indexMaterial < maxMaterialCount; ++indexMaterial)
                    {
                        combine.Add(new CombineInstance()
                        {
                            mesh = colorMesh,
                            transform = meshFilters[indexCombineInstance].transform.localToWorldMatrix,
                            subMeshIndex = indexMaterial
                        });
                    }
                }   
                
                Mesh newMesh = new Mesh();
                newMesh.indexFormat = IndexFormat.UInt32;
                newMesh.CombineMeshes(combine.ToArray());    

                GameObject tmpRendererBase = new GameObject("TMP_RENDERER BASE");

                m_disposeList.Add(tmpRendererBase);
                
                tmpRendererBase.AddComponent<MeshFilter>().sharedMesh = newMesh;
                tmpRendererBase.AddComponent<MeshRenderer>().enabled = false; // Disabled to avoid magenta

                List<Renderer> splitRenderers = new List<Renderer>();

                var splitMeshes = PerfectCullingMeshSplitting.Split(newMesh, Matrix4x4.identity);
                
                foreach (Mesh mesh in splitMeshes)
                {
                    GameObject tmpRenderer = new GameObject("TMP_RENDERER");

                    tmpRenderer.AddComponent<MeshFilter>().sharedMesh = mesh;
                    MeshRenderer mr = tmpRenderer.AddComponent<MeshRenderer>();
                    mr.enabled = false; // Disabled to avoid magenta
                    
                    splitRenderers.Add(mr);
                    
                    m_disposeList.Add(tmpRenderer);
                }

                allRenderers = splitRenderers;
            }

            m_namedPipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 4096 * 1, 4096 * 1);

            Vector3[] samplingLocations = perfectCullingBakeSettings.SamplingLocations.Where(x => x.Active).Select(x => x.Position).ToArray();

            pc_renderer.Render(new pc_renderer.pc_renderer_settings()
            {
                SamplingPositions = samplingLocations,
                NativeMeshRenderers = PrepareNativeMeshRendererData(allRenderers, groupMapping)
            });
            
            // This blocks the main thread but the Async variants doesn't appear to be supported
            m_namedPipeServer.WaitForConnection();

            m_pipeStreamReader = new BinaryReader(m_namedPipeServer);
            m_pipeStreamWriter = new BinaryWriter(m_namedPipeServer);
            
            if (m_pipeStreamReader.Read() != StatusHello)
            {
                throw new Exception("Communication failed");
            }
            
            m_pipeStreamWriter.Write(PerfectCullingSettings.Instance.bakeCameraResolutionWidth);
            m_pipeStreamWriter.Write(PerfectCullingSettings.Instance.bakeCameraResolutionHeight);
        }

        public override PerfectCullingBakerHandle SamplePosition(Vector3 pos)
        {
            int count = m_pipeStreamReader.ReadInt32();

            int[] inputHashes = new int[count];

            for (int i = 0; i < count; ++i)
            {
                inputHashes[i] = m_pipeStreamReader.ReadInt32();
            }
            
            return new PerfectCullingBakerNativeWin64Handle()
            {
                inputHashes = inputHashes,
                m_hash = m_sceneColor.Hash
            };
        }

        private pc_renderer.NativeMeshRenderers[] PrepareNativeMeshRendererData(List<Renderer> inputRenderers, Dictionary<Renderer, int> groupMappingDict)
        {
            Dictionary<Mesh, List<Renderer>> meshDict = new Dictionary<Mesh, List<Renderer>>();

            foreach (Renderer renderer in inputRenderers)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();

                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                if (meshDict.TryGetValue(meshFilter.sharedMesh, out List<Renderer> listWithRenderers))
                {
                    listWithRenderers.Add(renderer);
                    
                    continue;
                }
                
                meshDict.Add(meshFilter.sharedMesh, new List<Renderer>() { renderer});
            }
            
            List<pc_renderer.NativeMeshRenderers> tmpMeshRenderers = new List<pc_renderer.NativeMeshRenderers>();

            foreach (KeyValuePair<Mesh, List<Renderer>> kv in meshDict)
            {
                List<Renderer> renderersWithThisMesh = kv.Value;
                Mesh thisMesh = kv.Key;
                
                Vector3[] verts = thisMesh.vertices;
                int[] indices = thisMesh.GetIndices(0);
                Color[] colors = thisMesh.colors;

                pc_renderer.NativeMeshData nativeMeshData = new pc_renderer.NativeMeshData()
                {
                    vertCount = verts.Length,
                    verts = verts,
                    
                    indCount = indices.Length,
                    indices = indices,
                    
                    colorCount = colors.Length,
                    colors = colors
                };

                List<pc_renderer.NativeRendererTransformation> tmpTransformations =
                    new List<pc_renderer.NativeRendererTransformation>(renderersWithThisMesh.Count);
                
                foreach (Renderer renderer in renderersWithThisMesh)
                { 
                    Matrix4x4 localToWorldMat = renderer.localToWorldMatrix;
                    Bounds bounds = renderer.bounds;
                    
                    pc_renderer.NativeRendererTransformation tf = new pc_renderer.NativeRendererTransformation()
                    {
                        boundsCenter = bounds.center,
                        boundsSize = bounds.size,
                        
                        mat4x4 = localToWorldMat
                    };
                    
                    tmpTransformations.Add(tf);
                }

                pc_renderer.NativeMeshRenderers nativeMeshRenderers = new pc_renderer.NativeMeshRenderers()
                {
                    meshData = nativeMeshData,
                    
                    transformations = tmpTransformations.ToArray(),
                    transformationCount = tmpTransformations.Count
                };
                
                tmpMeshRenderers.Add(nativeMeshRenderers);
            }
            
            return tmpMeshRenderers.ToArray();
        }

        public override void Dispose()
        {
            m_sceneColor.Dispose();
            
            if (m_pipeStreamReader != null)
            {
                m_pipeStreamReader.Dispose();
            }  
            
            if (m_pipeStreamWriter != null)
            {
                m_pipeStreamWriter.Dispose();
            }
            
            if (m_namedPipeServer != null)
            {
                m_namedPipeServer.Dispose();
            }

            foreach (UnityEngine.Object obj in m_disposeList)
            {
                Object.DestroyImmediate(obj);
            }
            
            m_disposeList.Clear();
            m_disposeList = null;
            
            pc_renderer.JoinThread();
        }
    }
}