// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling
{
    [RequireComponent(typeof(Camera))]
    public class PerfectCullingCamera : MonoBehaviour
    {
        public static List<PerfectCullingCamera> AllCameras = new List<PerfectCullingCamera>();
        
        public bool ShowInGameStats { get; set; }
        
        public bool VisualizeFrustumCulling { get; set; } = true;
        
        public int LastTotalVertices { get; private set; }
        public int LastVisibleVertices  { get; private set; }
        public int LastCulledVertices => LastTotalVertices - LastVisibleVertices;
        
        public int LastVisible { get; private set; }
        public int LastTotal { get;private set; }
        public int LastCulled => LastTotal - LastVisible;

        [Tooltip("Allows to take into account neighbor cells to prevent popping issues. It's a great way to compensate for a too sparse bake. This comes with a minor performance impact.\n\n" +
                 "You can achieve even better results without performance implications by baking this in by using the Merge-Downsample feature for your bakes.")]
        [Range(0, 2)]
        public int NeighborCellIncludeRadius = 0;

        public PerfectCullingVisibilityLayer visibilityLayer = PerfectCullingVisibilityLayer.All;

        public int LastFrameHash => m_lastFrameHash;

        private bool m_invertCulling = false;

        public bool InvertCulling
        {
            get => m_invertCulling;
            set
            {
                m_invertCulling = value;
                
                SetDirty();
            }
        }

        // We use this as a more efficient HashSet
        private static readonly bool[] m_visibleRenderers = new bool[PerfectCullingConstants.MaxRenderers + 1];

        private static int m_lastFrame = -1;

        private static int m_lastFrameHash = -1;

        private Camera m_camera;

        private static Vector3[] m_offsets = new Vector3[]
        {
            // 0, 0, 0 needs to be first because IncludeNeighborCells depends on that
            new Vector3(0, 0, 0),
            
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),

            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
        };

        void Awake()
        {
            m_camera = GetComponent<Camera>();
        }
        
        private void OnEnable()
        {
            SetDirty();
            
            AllCameras.Add(this);

            // https://issuetracker.unity3d.com/issues/hdrp-renderpipelinemanager-dot-currentpipeline-is-null-for-the-first-few-frames-of-playmode
            // Concluding that we CANNOT check for if (RenderPipelineManager.currentPipeline != null) to detect SRP here
            
#if UNITY_2019_1_OR_NEWER
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
#endif
            Camera.onPreCull += CamPreCull;
        }

        private void OnDisable()
        {
            // Lets make sure we do this first to not get into a situation where we didn't unsubscribe
#if UNITY_2019_1_OR_NEWER
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
#endif
            Camera.onPreCull -= CamPreCull;
            
#if UNITY_EDITOR
            RestoreSceneCamerasCullingMatrices();
#endif
            
            SetDirty();
            
            // Execute this before ToggleAllRenderers - just in case we run into an exception.
            AllCameras.Remove(this);

            // Toggle everything back on. Just in case.
            foreach (var volume in PerfectCullingVolume.AllVolumes)
            {
                volume.QueueToggleAllRenderers(true);
                
                // Take effect immediately
                // We also force null checks because OnDisable() might have been called as part of an active destruction process (scene change, etc.)
                volume.ExecuteQueue(true);
            }
            
            LastTotal = 0;
            LastVisible = 0;
            
            System.Array.Clear(m_visibleRenderers, 0, m_visibleRenderers.Length);
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            CamPreCull(camera);
        }

#if UNITY_EDITOR
        private readonly string m_LateUpdate_SampleName = nameof(PerfectCullingCamera) + "." + nameof(CameraFrustumVisualizationEditorOnly);

        private void LateUpdate()
        {
            // Doing it in an update loop is pretty stupid but HDRP doesn't allow us to update this information just in time...
            
            Profiler.BeginSample(m_LateUpdate_SampleName);
            {
                CameraFrustumVisualizationEditorOnly();
            }
            Profiler.EndSample();
        }

        private void CameraFrustumVisualizationEditorOnly()
        {
            int selectedCamerasWithVisualizeFrustumCullingEnabledCount = 0;
            
            foreach (PerfectCullingCamera otherCamera in AllCameras)
            {
                selectedCamerasWithVisualizeFrustumCullingEnabledCount += (otherCamera.VisualizeFrustumCulling && UnityEditor.Selection.activeGameObject == otherCamera.gameObject) ? 1 : 0;
            }

            if (selectedCamerasWithVisualizeFrustumCullingEnabledCount <= 0)
            {
                RestoreSceneCamerasCullingMatrices();

                return;
            }
            
            if (UnityEditor.Selection.activeGameObject != m_camera.gameObject)
            {
                return;
            }

            // This allocates but it is Editor Only code
            foreach (Camera sceneCamera in UnityEditor.SceneView.GetAllSceneCameras())
            {
                sceneCamera.cullingMatrix = m_camera.cullingMatrix;
            }
        }

        private void RestoreSceneCamerasCullingMatrices()
        {
            foreach (Camera sceneCamera in UnityEditor.SceneView.GetAllSceneCameras())
            {
                sceneCamera.ResetCullingMatrix();
            }
        }
#endif

        private readonly string m_CamPreCull_SampleName = nameof(PerfectCullingCamera) + "." + nameof(PerformCameraCulling);
        
        private void CamPreCull(Camera camera)
        {
            Profiler.BeginSample(m_CamPreCull_SampleName);
            {
                PerformCameraCulling(camera);
            }
            Profiler.EndSample();
        }

        private void PerformCameraCulling(Camera camera)
        {
            if (camera != m_camera)
            {
                // Another camera rendering. We are not interested in it.
                return;
            }

            Vector3 camPos = transform.position;

            // We calculate a hash for all visible cell indices to tell whether our camera is dirty or not.
            int thisFrameHash = 13;

            int maxSamples = NeighborCellIncludeRadius != 0 ? m_offsets.Length : 1;
            
            foreach (var volume in PerfectCullingVolume.AllVolumes)
            {
                if (((int)volume.visibilityLayer & (int)visibilityLayer) == 0)
                {
                    // Not assigned to the same layer. We just ignore it.
                    continue;
                }
                
                for (int neighborIndex = 0; neighborIndex < maxSamples; ++neighborIndex)
                {
                    for (int j = 1; j <= NeighborCellIncludeRadius + 1; ++j)
                    {
                        unchecked
                        {
                            int index = volume.GetIndexForWorldPos(camPos + Vector3.Scale(
                                m_offsets[neighborIndex] * j, volume.volumeBakeData.cellSize), out bool isOutOfBounds);

                            if ((volume.outOfBoundsBehaviour == PerfectCullingBakingBehaviour.EOutOfBoundsBehaviour.Cull || volume.outOfBoundsBehaviour == PerfectCullingBakingBehaviour.EOutOfBoundsBehaviour.IgnoreDoNothing) && isOutOfBounds)
                            {
                                continue;
                            }

                            thisFrameHash = thisFrameHash * 17 + index;
                        }
                    }
                }
            }

            // Hashes match. Nothing to do.
            if (m_lastFrameHash == thisFrameHash)
            {
                return;
            }
            
            // Only want to toggle everything off once per frame.
            // This makes sure that we don't disable renderers that another camera enabled before us.
            if (Time.frameCount != m_lastFrame)
            {
                foreach (var volume in PerfectCullingVolume.AllVolumes)
                {
                    // We don't execute this just yet because we want to not disable a renderer just to turn it back on again anyway
                    // So we only queue it but don't execute it yet to prevent costs for an unnecessary toggle
                    volume.QueueToggleAllRenderers(InvertCulling);
                }
                
                m_lastFrame = Time.frameCount;
            }
            
            LastTotal = 0;
            LastVisible = 0;

            int totalVisible = 0;

            m_lastFrameHash = thisFrameHash;

            LastTotalVertices = 0;
            LastVisibleVertices = 0;
            
            foreach (var volume in PerfectCullingVolume.AllVolumes)
            {
                if (((int)volume.visibilityLayer & (int)visibilityLayer) == 0)
                {
                    // Not assigned to the same layer. We just ignore it.
                    continue;
                }
                
                // Just maintain current state if we are supposed to ignore this volume (don't want to call Execute on the volume).
                {
                    _ = volume.GetIndexForWorldPos(camPos, out bool isOutOfBounds);

                    if ((volume.outOfBoundsBehaviour == PerfectCullingBakingBehaviour.EOutOfBoundsBehaviour.IgnoreDoNothing) && isOutOfBounds)
                    {
                        continue;
                    }
                }

                System.Array.Clear(m_visibleRenderers, 0, m_visibleRenderers.Length);

                LastTotalVertices += volume.TotalVertexCount;

                bool continueLoop = true;
                
                for (int neighborIndex = 0; (neighborIndex < maxSamples) && continueLoop; ++neighborIndex)
                {
                    // We don't check continueLoop here because we break out of this loop
                    for (int j = 1; (j <= NeighborCellIncludeRadius + 1); ++j)
                    {
                        volume.GetIndexForWorldPos(camPos + Vector3.Scale(
                            m_offsets[neighborIndex] * j, volume.volumeBakeData.cellSize), out bool isOutOfBounds);

                        if (volume.outOfBoundsBehaviour == PerfectCullingBakingBehaviour.EOutOfBoundsBehaviour.Cull && isOutOfBounds)
                        {
                            continue;
                        }
                        
                        PerfectCullingTemp.ListUshort.Clear();
                        volume.GetIndicesForWorldPos(
                            camPos + Vector3.Scale(m_offsets[neighborIndex] * j,
                                volume.volumeBakeData.cellSize), PerfectCullingTemp.ListUshort);

                        // If we are standing on an empty cell we pull in all renderers so we do not cull the entire world
                        // NOTE: We only do this for the primary cell (neighborIndex == 0) and not for neighbour cells to not impact performance!
                        if ((volume.emptyCellCullBehaviour == EEmptyCellCullBehaviour.CullNothing) && neighborIndex == 0 && PerfectCullingTemp.ListUshort.Count == 0)
                        {
                            int bakeGroupCount = volume.bakeGroups.Length;
                            
                            for (int index = 0;
                                index < bakeGroupCount;
                                ++index)
                            {
                                // We only queue this up for now
                                volume.QueueToggleRenderer(index, !InvertCulling, out PerfectCullingBakeGroup r);

                                m_visibleRenderers[index] = true;

                                LastVisibleVertices += r.vertexCount;
                                ++totalVisible;
                            }

                            continueLoop = false;
                            
                            break;
                        }

                        for (int indexIndices = 0; indexIndices < PerfectCullingTemp.ListUshort.Count; ++indexIndices)
                        {
                            int index = PerfectCullingTemp.ListUshort[indexIndices];

                            if (m_visibleRenderers[index])
                            {
                                // Already processed
                                continue;
                            }

                            // We only queue this up for now
                            volume.QueueToggleRenderer(index, !InvertCulling, out PerfectCullingBakeGroup r);

                            m_visibleRenderers[index] = true;

                            LastVisibleVertices += r.vertexCount;
                            ++totalVisible;
                        }
                    }
                }

                LastTotal += volume.RenderersCount;
                
                // Finally we can execute the queue
                volume.ExecuteQueue();
            }
            
            LastVisible += totalVisible;
        }

        void SetDirty()
        {
            m_lastFrameHash = -1;
        }
        
#if UNITY_EDITOR
        private int m_guiLastFrameHash = -1;
        private string m_guiLastText = null;

        private readonly string m_OnGUI_SampleName = nameof(PerfectCullingCamera) + "." + nameof(OnGUIEditorOnly);
        
        private void OnGUI()
        {
            Profiler.BeginSample(m_OnGUI_SampleName);
            {
                OnGUIEditorOnly();
            }
            Profiler.EndSample();
        }

        private void OnGUIEditorOnly()
        {
            if (!ShowInGameStats)
            {
                return;
            }
            
            if (m_guiLastFrameHash != LastFrameHash || m_guiLastText == null)
            {
                m_guiLastText = "* Perfect Culling Stats *\n"
                                + $"Total renderers: {LastTotal}\n"

                                + $" - Culled: {LastCulled} ({Mathf.Round((LastCulled / (float) LastTotal) * 100f)}%)\n"
                                + $" - Visible: {LastVisible}\n"
                                + $" - Culled verts: {PerfectCullingUtil.FormatNumber(LastCulledVertices)}/{PerfectCullingUtil.FormatNumber(LastTotalVertices)} ({Mathf.Round((LastCulledVertices / (float) LastTotalVertices) * 100f)}%)\n"
                                + $" - Hash: {LastFrameHash}\n";

                m_guiLastFrameHash = LastFrameHash;
            }

            GUI.skin.box.fontSize = 30;
            GUI.skin.box.alignment = TextAnchor.UpperLeft;

            GUILayout.Box(m_guiLastText, System.Array.Empty<GUILayoutOption>());
        }
#endif
    }
}