// Perfect Culling (C) 2021 Patrick König
//

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Koenigz.PerfectCulling
{
    /// <summary>
    /// This class contains all the available API. Anything that is not part of this class is not considered to be part of the official API and thus is more likely to change in future versions.
    /// </summary>
    public static class PerfectCullingAPI
    {
        /// <summary>
        /// Bake related API
        /// </summary>
        public static class Bake
        {
#if UNITY_EDITOR
            public static System.Action<PerfectCullingBakingBehaviour> OnBakeFinished;
            public static System.Action OnAllBakesFinished;

            public static T[] FindAllBakingBehaviours<T>() 
                where T : PerfectCullingBakingBehaviour
                => Object.FindObjectsOfType<T>();
            
            public static PerfectCullingBakingBehaviour[] FindAllBakingBehaviours() 
                => FindAllBakingBehaviours<PerfectCullingBakingBehaviour>();
            
            /// <summary>
            /// Schedules a bake but doesn't perform the bake yet. Call BakeAllScheduled to start the baking process for all scheduled bakes.
            /// </summary>
            /// <param name="bakeInformation">Information about the bake to schedule</param>
            public static void ScheduleBake(BakeInformation bakeInformation)
                => PerfectCullingBakingManager.ScheduleBake(bakeInformation);

            /// <summary>
            /// Starts to bake multiple baking behaviours immediately.
            /// </summary>
            /// <param name="cullingBakingBehaviours">All baking behaviours to bake</param>
            /// <param name="additionalOccluders">Additional occluders</param>
            public static void BakeNow(PerfectCullingBakingBehaviour[] cullingBakingBehaviours, HashSet<Renderer> additionalOccluders = null)
                => PerfectCullingBakingManager.BakeNow(cullingBakingBehaviours, additionalOccluders);

            /// <summary>
            /// Starts to bake a single baking behaviour immediately.
            /// </summary>
            /// <param name="bakingBehaviour">Baking behaviour to bake</param>
            /// <param name="additionalOccluders">Additional occluders</param>
            public static void BakeNow(PerfectCullingBakingBehaviour bakingBehaviour, HashSet<Renderer> additionalOccluders = null)
                => PerfectCullingBakingManager.BakeNow(bakingBehaviour, additionalOccluders);

            /// <summary>
            /// Starts to bake all scheduled bakes.
            /// </summary>
            public static void BakeAllScheduled()
                => PerfectCullingBakingManager.BakeAllScheduled();

            /// <summary>
            /// Bakes a multi-scene setup by temporarily merging all scenes.
            /// </summary>
            /// <param name="scenes">Scenes that should be included in this bake</param>
            public static void BakeMultiScene(List<Scene> scenes)
                => PerfectCullingBakingManager.BakeMultiScene(scenes);

            /// <summary>
            /// Allows to bake a single view point. This might be handy for some editor utilities.
            /// WARNING: THIS CHANGES YOUR SCENE!!!
            /// </summary>
            public static HashSet<Renderer> BakeSingleViewPointNow(Vector3 position, List<Renderer> inputRenderers)
            {
                HashSet<Renderer> visibleRenderers = new HashSet<Renderer>();
                
                GameObject tmp = new GameObject();

                tmp.transform.position = position;

                PerfectCullingVolume vol = tmp.AddComponent<PerfectCullingVolume>();

                vol.volumeSize = Vector3.one;
                vol.bakeCellSize = Vector3.one;
                
                PerfectCullingVolumeBakeData bakeData = ScriptableObject.CreateInstance<PerfectCullingVolumeBakeData>();

                vol.SetBakeData(bakeData);
                
                vol.bakeGroups = PerfectCullingUtil.CreateBakeGroupsForRenderers(inputRenderers, (x) => true).ToArray();

                var enumerator = vol.PerformBakeAsync(false, false, null);

                while (enumerator.MoveNext())
                {
                    // We block the main thread
                }
                
                PerfectCullingTemp.ListUshort.Clear();
                vol.BakeData.SampleAtIndex(0, PerfectCullingTemp.ListUshort);

                foreach (ushort index in PerfectCullingTemp.ListUshort)
                {
                    foreach (Renderer renderer in vol.bakeGroups[index].renderers)
                    {
                        visibleRenderers.Add(renderer);
                    }
                }
                
                Object.DestroyImmediate(vol.BakeData);
                Object.DestroyImmediate(tmp);
                
                return visibleRenderers;
            }
#endif
        }

        /// <summary>
        /// BakeGroup related API
        /// </summary>
        public static class BakeGroup
        {
            /// <summary>
            /// Returns number of renderers that are part of PerfectCullingBakeGroup
            /// </summary>
            public static int GetRendererCount(PerfectCullingBakeGroup group)
                => group.GetRuntimeRendererCount();
            
            /// <summary>
            /// Returns whether specified renderer is part of PerfectCullingBakeGroup
            /// </summary>
            public static bool ContainsRenderer(PerfectCullingBakeGroup group, Renderer r)
                => group.ContainsRuntimeRenderer(r);
            
            /// <summary>
            /// Adds specified renderer from PerfectCullingBakeGroup
            /// </summary>
            public static void AddRenderer(PerfectCullingBakeGroup group, Renderer r)
                => group.PushRuntimeRenderer(r);
            
            /// <summary>
            /// Removes specified renderer from PerfectCullingBakeGroup
            /// </summary>
            public static bool RemoveRenderer(PerfectCullingBakeGroup group, Renderer r)
                => group.PopRuntimeRenderer(r);
            
            /// <summary>
            /// Clears all renderers in PerfectCullingBakeGroup
            /// </summary>
            public static void Clear(PerfectCullingBakeGroup group) 
                => group.ClearRuntimeRenderers();
        }

        /// <summary>
        /// BakingBehaviour (for instance PerfectCullingVolume) API
        /// </summary>
        public static class BakingBehaviour
        {
            /// <summary>
            /// Returns List of PerfectCullingBakeGroup that contain the given renderer
            /// </summary>
            public static void FindBakeGroupWithRenderer(PerfectCullingBakingBehaviour behaviour, Renderer r, List<PerfectCullingBakeGroup> result)
            {
                if (result == null)
                {
                    result = new List<PerfectCullingBakeGroup>();
                }
                else
                {
                    result.Clear();
                }
                
                int length = behaviour.bakeGroups.Length;
                
                for (int i = 0; i < length; ++i)
                {
                    if (behaviour.bakeGroups[i].ContainsRuntimeRenderer(r))
                    {
                        result.Add(behaviour.bakeGroups[i]);
                    }
                }
            }
        }
    }
}