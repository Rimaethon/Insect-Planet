// Perfect Culling (C) 2021 Patrick König
//

using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingBakerUnity : PerfectCullingBaker
    {
        private PerfectCullingSceneColor m_sceneColor;
        
        private readonly Camera m_cam;
        private readonly Transform m_camTransform;
        
        private readonly int m_combinedImageWidth;
        private readonly int m_combinedImageHeight;
        
        private readonly ComputeShader m_imageComputeShader;

        private readonly int m_kernelMain;
        private readonly int m_kernelExtract;

        private readonly RenderTexture m_outputHashRT;
        
        // Constant values
        private readonly Quaternion[] m_camBakeRotations;
        private readonly Rect[] m_camBakeRects;
        
        // Shader properties
        private readonly int m_propInput = Shader.PropertyToID("Input");
        private readonly int m_propOutputWrite = Shader.PropertyToID("Output_Write");
        private readonly int m_propOutputRead= Shader.PropertyToID("Output_Read");
        private readonly int m_propAppendDataBuffer = Shader.PropertyToID("AppendDataBuffer");

        private readonly RenderPipelineAsset m_activeGraphicsPipeline;
        private readonly RenderPipelineAsset m_activeQualityPipeline;
        
        public PerfectCullingBakerUnity(PerfectCullingBakeSettings perfectCullingBakeSettings) : 
            base(perfectCullingBakeSettings)
        {
            m_activeGraphicsPipeline = GraphicsSettings.renderPipelineAsset;
            GraphicsSettings.renderPipelineAsset = null;
            
#if UNITY_2019_3_OR_NEWER
            m_activeQualityPipeline = QualitySettings.renderPipeline;
            QualitySettings.renderPipeline = null;
#endif
            
#if UNITY_EDITOR
            m_imageComputeShader = PerfectCullingResourcesLocator.Instance.PointExtractorComputeShader;
#endif
            
            if (m_imageComputeShader == null)
            {
                Debug.LogError("Unable to locate Perfect Culling Compute Shader. Did you move any of the assets to a different location?");

                return;
            }

            // This destroys existing cameras. Make sure this is called before spawning our own.
            // Also disables all renderers. Make sure this is called before enabling the actual renderers.
            PrepareScene();
            
            m_sceneColor = new PerfectCullingSceneColor(perfectCullingBakeSettings.Groups, m_bakeSettings.AdditionalOccluders);

            // Prepare RenderTexture
            m_outputHashRT = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_outputHashRT.autoGenerateMips = false;
            m_outputHashRT.enableRandomWrite = true;
            //m_outputHashRT.volumeDepth = 256;
            //m_outputHashRT.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            m_outputHashRT.Create();
            
            int camFaceWidth = perfectCullingBakeSettings.Width;
            int camFaceHeight = perfectCullingBakeSettings.Height;
            
            m_combinedImageWidth = camFaceWidth * 3;
            m_combinedImageHeight = camFaceHeight * 2;
            
            m_cam = SpawnCamera();
            m_camTransform = m_cam.transform;

            // We pre-calculate these values so we can reuse them
            m_camBakeRotations = new Quaternion[]
            {
                Quaternion.Euler(0f, 90f, 0f),
                Quaternion.Euler(0f, -90f, 0f),
                Quaternion.Euler(-90f, 0f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                Quaternion.Euler(0f, 180f, 0f),
                Quaternion.Euler(0f, 0f, 0f)
            };
            
            m_camBakeRects = new Rect[]
            {
                new Rect(camFaceWidth * 0, camFaceHeight * 0, camFaceWidth, camFaceHeight),
                new Rect(camFaceWidth * 1, camFaceHeight * 0, camFaceWidth, camFaceHeight),
                new Rect(camFaceWidth * 2, camFaceHeight * 0, camFaceWidth, camFaceHeight),
                new Rect(camFaceWidth * 0, camFaceHeight * 1, camFaceWidth, camFaceHeight),
                new Rect(camFaceWidth * 1, camFaceHeight * 1, camFaceWidth, camFaceHeight),
                new Rect(camFaceWidth * 2, camFaceHeight * 1, camFaceWidth, camFaceHeight)
            };

            // Find all kernels
            m_kernelMain = m_imageComputeShader.FindKernel("CSMain");
            m_kernelExtract = m_imageComputeShader.FindKernel("CSExtract");
        }

        public override void Dispose()
        {
            GraphicsSettings.renderPipelineAsset = m_activeGraphicsPipeline;
            
#if UNITY_2019_3_OR_NEWER
            QualitySettings.renderPipeline = m_activeQualityPipeline;
#endif
            
            m_sceneColor.Dispose();
            
            m_sceneColor = null;
            
            GameObject.DestroyImmediate(m_outputHashRT);
            GameObject.DestroyImmediate(m_cam.gameObject);
        }

        void PrepareScene()
        {
            Camera[] allCams = Object.FindObjectsOfType<Camera>();

            foreach (Camera cam in allCams)
            {
                cam.enabled = false;
            }
            
            // Attempt to bypass light culling
            Light[] allLights = Object.FindObjectsOfType<Light>();
 
            foreach (Light light in allLights)
            {
                MonoBehaviour[] allMonos = light.GetComponents<MonoBehaviour>();
 
                // We manually need to find the UniversalAdditionalLightData because Destroy will fail otherwise...
                for (int i = allMonos.Length - 1; i >= 0; --i)
                {
                    MonoBehaviour m = allMonos[i];

                    if (m == null)
                    {
                        // Maybe a missing script...
                        continue;
                    }

                    if (!m.GetType().Name.ToLower().Contains("additional"))
                    {
                        continue;
                    }
 
                    Object.DestroyImmediate(m);
                }
 
                Object.DestroyImmediate(light);
            }
            
            ReflectionProbe[] allRefProbes = Object.FindObjectsOfType<ReflectionProbe>();

            foreach (ReflectionProbe probe in allRefProbes)
            {
                MonoBehaviour[] allMonos = probe.GetComponents<MonoBehaviour>();
 
                // We manually need to find the AdditionalReflectionData component because Destroy will fail otherwise...
                for (int i = allMonos.Length - 1; i >= 0; --i)
                {
                    MonoBehaviour m = allMonos[i];

                    if (m == null)
                    {
                        // Maybe a missing script...
                        continue;
                    }

                    if (!m.GetType().Name.ToLower().Contains("additional"))
                    {
                        continue;
                    }
 
                    Object.DestroyImmediate(m);
                }
                
                Object.DestroyImmediate(probe);
            }
            
            // Disable all renderers in scene
            // We do that to make it absolutely impossible that any of them could make it into our snapshot
            Renderer[] allRenderersInScene = Object.FindObjectsOfType<Renderer>();

            foreach (Renderer r in allRenderersInScene)
            {
                r.enabled = false;
            }
            
            // Same for Terrains
            UnityEngine.Terrain[] allTerrainsInScene = Object.FindObjectsOfType<UnityEngine.Terrain>();

            foreach (UnityEngine.Terrain t in allTerrainsInScene)
            {
                t.enabled = false;
            }
            
            RenderSettings.fog = false;

#if UNITY_2020_1_OR_NEWER && UNITY_EDITOR
            try
            {
                UnityEditor.Lightmapping.lightingSettings.autoGenerate = false;
            }
            catch (System.Exception)
            {
                // Somebody at Unity thought it was a great idea to throw an exception when accessing a property that is null (giving you no way to check for null because that causes an exception as well, of course)...
            }
#endif
        }
        
        public override PerfectCullingBakerHandle SamplePosition(Vector3 pos)
        {
            RenderTexture rtCam = RenderTexture.GetTemporary(m_combinedImageWidth, m_combinedImageHeight, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            m_cam.targetTexture = rtCam;
            
            rtCam.filterMode = FilterMode.Point;

            rtCam.Create();
            
            RenderTexture.active = rtCam;
            
            GL.Clear(true, true, PerfectCullingConstants.ClearColor);
            
            // This is a bit of a hot spot and as such this is already an unrolled for-loop.
            {
                m_camTransform.position = pos;

                // Face #1
                m_camTransform.localRotation = m_camBakeRotations[0];
                m_cam.pixelRect = m_camBakeRects[0];
                m_cam.Render();

                // Face #2
                m_camTransform.localRotation = m_camBakeRotations[1];
                m_cam.pixelRect = m_camBakeRects[1];
                m_cam.Render();

                // Face #3
                m_camTransform.localRotation = m_camBakeRotations[2];
                m_cam.pixelRect = m_camBakeRects[2];
                m_cam.Render();

                // Face #4
                m_camTransform.localRotation = m_camBakeRotations[3];
                m_cam.pixelRect = m_camBakeRects[3];
                m_cam.Render();

                // Face #5
                m_camTransform.localRotation = m_camBakeRotations[4];
                m_cam.pixelRect = m_camBakeRects[4];
                m_cam.Render();

                // Face #6
                m_camTransform.localRotation = m_camBakeRotations[5];
                m_cam.pixelRect = m_camBakeRects[5];
                m_cam.Render();
            }
            
            // This is slow but might be useful for debugging and hardware that doesn't support Compute Shaders.
            // This is super slow though.
            if (PerfectCullingSettings.Instance.useUnityForRenderingCpuCompute)
            {
                Texture2D cpuReadbackTxt = new Texture2D(m_combinedImageWidth, m_combinedImageHeight, TextureFormat.RGBA32, false);
                cpuReadbackTxt.ReadPixels(new Rect(0, 0, m_combinedImageWidth, m_combinedImageHeight), 0, 0);
                cpuReadbackTxt.Apply();

                Color32[] pixels = cpuReadbackTxt.GetPixels32();

                Object.DestroyImmediate(cpuReadbackTxt);

                RenderTexture.active = null;

                RenderTexture.ReleaseTemporary(rtCam);

                return new PerfectCullingBakerUnityCpuHandle()
                {
                    m_hash = m_sceneColor.Hash,
                    Pixels = pixels
                };
            }
            
#pragma warning disable 162
            bool condition = Mathf.Approximately(pos.x, 8.875f) && Mathf.Approximately(pos.y, 1.51f) && Mathf.Approximately(pos.z, -8.625f);
            
            if (false)
            {
                Texture2D newTxt = new Texture2D(m_combinedImageWidth, m_combinedImageHeight, TextureFormat.RGBA32, false);
                newTxt.ReadPixels(new Rect(0, 0, m_combinedImageWidth, m_combinedImageHeight), 0, 0);
                newTxt.Apply();

                byte[] bytes = newTxt.EncodeToPNG();

                // For testing purposes, also write to a file in the project folder
                System.IO.File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

                GameObject.DestroyImmediate(newTxt);

                Application.OpenURL(Application.dataPath + "/../SavedScreen.png");

                RenderTexture.active = null;
                
                RenderTexture.ReleaseTemporary(rtCam);
                
                throw new Exception();
            }
#pragma warning restore 162

            RenderTexture.active = null;
            
            PerfectCullingBakerHandle perfectCullingBakerHandle = GetResult(rtCam);

            RenderTexture.ReleaseTemporary(rtCam);
            
            return perfectCullingBakerHandle;
        }
        
        PerfectCullingBakerHandle GetResult(Texture input)
        {
            // Looks like we cannot really reuse the buffers and need to create them again and again
            ComputeBuffer computeAppendBuf = new ComputeBuffer(PerfectCullingConstants.MaxRenderers, sizeof(int), ComputeBufferType.Append);
            ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            // Also looks like we need to reset the counter value even though we create a new buffer
            computeAppendBuf.SetCounterValue(0);

            m_imageComputeShader.SetTexture(m_kernelMain, m_propInput, input);
            m_imageComputeShader.SetTexture(m_kernelMain, m_propOutputWrite, m_outputHashRT);

            // Kinda dumb that we need to set it twice but on OSX the write doesn't seem to become visible in the second kernel if we don't do it
            m_imageComputeShader.SetTexture(m_kernelExtract, m_propOutputWrite, m_outputHashRT);
            m_imageComputeShader.SetTexture(m_kernelExtract, m_propOutputRead, m_outputHashRT);
            
            m_imageComputeShader.SetBuffer(m_kernelExtract, m_propAppendDataBuffer, computeAppendBuf);
         
            // Dispatch
            // Especially on OSX we might be limited in our thread groups. Lets play it safe and stay below 256.
            m_imageComputeShader.Dispatch(m_kernelMain, m_combinedImageWidth / 16, m_combinedImageHeight / 16, 1);
            m_imageComputeShader.Dispatch(m_kernelExtract, 256 / 16, 256 / 16, 1);

            return new PerfectCullingBakerUnityHandle()
            {
                appendBuf = computeAppendBuf,
                countBuf = countBuffer,
                m_hash = m_sceneColor.Hash
            };
        }
        
        Camera SpawnCamera()
        {
            GameObject go = new GameObject("Perfect Culling Cam");
            
            Camera cam = go.AddComponent<Camera>();

            cam.nearClipPlane = 0.001f;
            cam.farClipPlane = 1000f;

            cam.fieldOfView = 90f;

            cam.allowMSAA = false;
            cam.allowHDR = false;
            cam.useOcclusionCulling = false;
            cam.allowDynamicResolution = false;

            cam.stereoTargetEye = StereoTargetEyeMask.None;
            cam.clearFlags = CameraClearFlags.Nothing; // We clear the RenderTexture before rendering to it
            cam.backgroundColor = Color.black;
            cam.aspect = 1f;
            cam.renderingPath = RenderingPath.Forward; // VertexLit seems to break rendering at times
            cam.cullingMask = 1 << PerfectCullingConstants.CamBakeLayer;
            cam.enabled = false;

            cam.forceIntoRenderTexture = true;
            
            //go.SetActive(false);

            return cam;
        }
    }
}