// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Koenigz.PerfectCulling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Koenigz.PerfectCulling
{
    public static class PerfectCullingBakingManager
    {
#pragma warning disable 0414
        public static bool IsBaking => m_activeBake != null;
        
        private static IEnumerator m_activeBake = null;
        private static PerfectCullingBakingBehaviour m_activeBakingBehaviour = null;
        
        private static readonly Queue<BakeInformation> m_scheduledBakes =
            new Queue<BakeInformation>();

#pragma warning restore 0414

        /// <summary>
        /// Schedules a bake but doesn't perform the bake yet. Call BakeAllScheduled to start the baking process for all scheduled bakes.
        /// </summary>
        /// <param name="bakeInformation">Information about the bake to schedule</param>
        public static void ScheduleBake(BakeInformation bakeInformation)
        {
            m_scheduledBakes.Enqueue(bakeInformation);
        }
        
        /// <summary>
        /// Starts to bake multiple baking behaviours immediately.
        /// </summary>
        /// <param name="cullingBakingBehaviours">All baking behaviours to bake</param>
        /// <param name="additionalOccluders">Additional occluders</param>
        public static void BakeNow(PerfectCullingBakingBehaviour[] cullingBakingBehaviours, HashSet<Renderer> additionalOccluders = null)
        {
            m_scheduledBakes.Clear();

            HashSet<PerfectCullingBakeData> bakeDatas = new HashSet<PerfectCullingBakeData>();

            foreach (PerfectCullingBakingBehaviour bakingBehaviour in cullingBakingBehaviours)
            {
                if (bakingBehaviour.BakeData == null)
                {
                    continue;
                }
                
                // Only want to bake once
                if (bakeDatas.Add(bakingBehaviour.BakeData))
                {
                    ScheduleBake(new BakeInformation()
                    {
                        BakingBehaviour = bakingBehaviour,
                        AdditionalOccluders = additionalOccluders
                    });
                }
            }
            
            BakeAllScheduled();
        }

        /// <summary>
        /// Starts to bake a single baking behaviour immediately.
        /// </summary>
        /// <param name="bakingBehaviour">Baking behaviour to bake</param>
        /// <param name="additionalOccluders">Additional occluders</param>
        public static void BakeNow(PerfectCullingBakingBehaviour bakingBehaviour, HashSet<Renderer> additionalOccluders = null)
        {
            m_scheduledBakes.Clear();
            
            ScheduleBake(new BakeInformation()
            {
                BakingBehaviour = bakingBehaviour,
                AdditionalOccluders = additionalOccluders
            });
            
            BakeAllScheduled();
        }

        /// <summary>
        /// Starts to bake all scheduled bakes.
        /// </summary>
        public static void BakeAllScheduled()
        {
#if UNITY_EDITOR
            if (m_scheduledBakes.Count <= 0)
            {
                Debug.LogError("Nothing to bake.");

                return;
            }

            UnityEditor.EditorApplication.update += EditorUpdate;
            
            BakeInformation bakeInformation = m_scheduledBakes.Dequeue();
            m_activeBakingBehaviour = bakeInformation.BakingBehaviour;
            m_activeBake = m_activeBakingBehaviour.PerformBakeAsync(false, true, bakeInformation.AdditionalOccluders);
#endif
        }

        private class SceneState
        {
            public string ScenePath;
            public bool IsLoaded;
        }
        
        public static void BakeMultiScene(List<Scene> scenes)
        {
#if UNITY_EDITOR
            List<SceneState> sceneStates = new List<SceneState>();
            List<Scene> actualScenes = new List<Scene>();
            
            for (int i = 0; i < scenes.Count; i++)
            {
                Scene scene = scenes[i];
                
                if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                {
                    // Exclude scenes that are not already saved on disk such as the untitled scene
                    
                    continue;
                }

                sceneStates.Add(new SceneState()
                {
                    IsLoaded = scene.isLoaded,
                    ScenePath = scenes[i].path,
                });

                actualScenes.Add(scene);
            }

            Scene[] relevantScenes = actualScenes.Where(x => x.isLoaded).ToArray();

            if (relevantScenes.Length != actualScenes.Count)
            {
                Debug.LogWarning("Some scenes are excluded because they have not been loaded.");
            }

            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveModifiedScenesIfUserWantsTo(relevantScenes))
            {
                return;
            }
            
            for (int i = 1; i < relevantScenes.Length; i++)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MergeScenes(relevantScenes[i], relevantScenes[0]);
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(relevantScenes[0], PerfectCullingConstants.MultiSceneTempPath);

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(PerfectCullingConstants.MultiSceneTempPath, UnityEditor.SceneManagement.OpenSceneMode.Single);

            PerfectCullingBakingBehaviour[] bakingBehaviours = UnityEngine.Object.FindObjectsOfType<PerfectCullingBakingBehaviour>();

            HashSet<Renderer> renderers = new HashSet<Renderer>();

            foreach (var b in bakingBehaviours)
            {
                foreach (var g in b.bakeGroups)
                {
                    foreach (var r in g.renderers)
                    {
                        renderers.Add(r);
                    }
                }
            }
            
            void OnMultiBakeFinished()
            {
                try
                {
                    foreach (SceneState sceneState in sceneStates)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneState.ScenePath, sceneState.IsLoaded ? UnityEditor.SceneManagement.OpenSceneMode.Additive : UnityEditor.SceneManagement.OpenSceneMode.AdditiveWithoutLoading);
                    }

                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(
                        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), true);

                    UnityEditor.AssetDatabase.DeleteAsset(PerfectCullingConstants.MultiSceneTempPath);
                }
                finally
                {
                    PerfectCullingAPI.Bake.OnAllBakesFinished -= OnMultiBakeFinished;
                }
            }
            
            PerfectCullingAPI.Bake.OnAllBakesFinished += OnMultiBakeFinished;
            
            PerfectCullingBakingManager.BakeNow(bakingBehaviours, renderers);
#endif
        }

        private static void EditorUpdate()
        {
#if UNITY_EDITOR
            bool needsSceneReload = true;
            
            // Null check to make sure we unsubscribe at the end if this is invalid.
            if (m_activeBake != null)
            {
                if (m_activeBake.MoveNext())
                {
                    if (m_activeBake.Current is PerfectCullingBakeAbortedYieldInstruction)
                    {
                        Debug.Log("Bake(s) aborted.");
                        
                        // Abort all scheduled bakes, too.
                        m_scheduledBakes.Clear();
                    }
                    else if (m_activeBake.Current is PerfectCullingBakeNotStartedYieldInstruction)
                    {
                        needsSceneReload = false;
                        
                        Debug.Log("Bake was not started. Aborting all bakes.");
                        
                        // Abort all scheduled bakes, too.
                        m_scheduledBakes.Clear();
                    }
                    else
                    {
                        return;
                    }
                }

                if (m_activeBake is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                
                PerfectCullingAPI.Bake.OnBakeFinished?.Invoke(m_activeBakingBehaviour);

                if (m_scheduledBakes.Count > 0)
                {
                    BakeInformation bakeInformation = m_scheduledBakes.Dequeue();
                    m_activeBakingBehaviour = bakeInformation.BakingBehaviour;
                    m_activeBake = m_activeBakingBehaviour.PerformBakeAsync(false, false, bakeInformation.AdditionalOccluders);

                    return;
                }

                m_activeBake = null;
                m_activeBakingBehaviour = null;
            }

            UnityEditor.EditorApplication.update -= EditorUpdate;

            if (needsSceneReload && PerfectCullingConstants.AllowSceneReload)
            {
                string scenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            }

            PerfectCullingAPI.Bake.OnAllBakesFinished?.Invoke();
#endif
        }
    }
}
