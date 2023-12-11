// Perfect Culling (C) 2021 Patrick König
//

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using Koenigz.PerfectCulling;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingMenuOptions
    {
        [UnityEditor.MenuItem("Perfect Culling/Bake/Bake all", false, 0)]
        private static void BakeAll()
        {
            if (UnityEditor.SceneManagement.EditorSceneManager.sceneCount <= 1)
            {
                BakeSingle();
            }
            else
            {
                BakeMulti();
            }
        }

        private static void BakeSingle()
        {
            Debug.Log("Single scene bake.");
            
            PerfectCullingBakingBehaviour[] bakingBehaviours = GameObject.FindObjectsOfType<PerfectCullingBakingBehaviour>();

            PerfectCullingBakingManager.BakeNow(bakingBehaviours);
        }

        private static void BakeMulti()
        {
            Debug.Log("Multi scene bake.");
            
            List<Scene> scenesToBake = new List<Scene>();
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);

                scenesToBake.Add(scene);
            }
            
            PerfectCullingBakingManager.BakeMultiScene(scenesToBake);
        }

        
        [UnityEditor.MenuItem("Perfect Culling/Create/Baking volume", false, 0)]
        private static void CreateNewBakingVolume()
        {
            GameObject go = new GameObject("PerfectCulling Baking Volume");

            PerfectCullingVolume vol = go.AddComponent<PerfectCullingVolume>();

            vol.bakeCellSize = new Vector3(10, 5, 10);
            vol.volumeBakeBounds = new Bounds(Vector3.zero, new Vector3(100, 5, 100));

            UnityEditor.Selection.activeObject = go;
            UnityEditor.SceneView.lastActiveSceneView.Frame(vol.volumeBakeBounds, false);
        }
        
        [UnityEditor.MenuItem("Perfect Culling/Create/Exclude volume", false, 0)]
        private static void CreateNewExcludeVolume()
        {
            GameObject go = new GameObject("PerfectCulling Exclude Volume");

            PerfectCullingExcludeVolume vol = go.AddComponent<PerfectCullingExcludeVolume>();

            vol.volumeExcludeBounds = new Bounds(Vector3.zero, new Vector3(100, 5, 100));

            UnityEditor.Selection.activeObject = go;
            UnityEditor.SceneView.lastActiveSceneView.Frame(vol.volumeExcludeBounds, false);
        }
        
        [UnityEditor.MenuItem("Perfect Culling/Create/Always Include volume", false, 0)]
        private static void CreateNewAlwaysIncludeVolume()
        {
            GameObject go = new GameObject("PerfectCulling Always Include Volume");

            PerfectCullingAlwaysIncludeVolume vol = go.AddComponent<PerfectCullingAlwaysIncludeVolume>();

            vol.volumeIncludeBounds = new Bounds(Vector3.zero, new Vector3(100, 5, 100));

            UnityEditor.Selection.activeObject = go;
            UnityEditor.SceneView.lastActiveSceneView.Frame(vol.volumeIncludeBounds, false);
        }

        [UnityEditor.MenuItem("Perfect Culling/Create/Portal cell", false, 0)]
        private static void CreatePortalCell()
        {
            GameObject go = new GameObject("PerfectCulling Portal Cell");

            go.AddComponent<PerfectCullingPortalCell>();
            
            UnityEditor.Selection.activeObject = go;
        }
        
        [UnityEditor.MenuItem("Perfect Culling/Settings")]
        private static void SelectSettings()
        {
            UnityEditor.Selection.activeObject = PerfectCullingSettings.Instance;
        }
    }
}
#endif