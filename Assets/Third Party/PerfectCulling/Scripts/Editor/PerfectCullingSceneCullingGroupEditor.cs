// Perfect Culling (C) 2021 Patrick König
//

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Koenigz.PerfectCulling
{
    [CustomEditor((typeof(PerfectCullingSceneGroup)))]
    public class PerfectCullingSceneCullingGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PerfectCullingSceneGroup monoCullingGroup = target as PerfectCullingSceneGroup;
            
            if (GUILayout.Button("Collect children"))
            {
                UnityEditor.Undo.RecordObject(monoCullingGroup, "Collected children in SceneCullingGroup");

                monoCullingGroup.SetRenderers(monoCullingGroup.GetComponentsInChildren<Renderer>());
                
                EditorUtility.SetDirty(monoCullingGroup);
            }
            
            if (GUILayout.Button("Clear"))
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Clear all renderers in this group?",
                    "This will clear all renderers in this group.", "OK", "Cancel"))
                {
                    UnityEditor.Undo.RecordObject(monoCullingGroup, "Cleared all renderers in SceneCullingGroup");
                    
                    monoCullingGroup.SetRenderers(System.Array.Empty<Renderer>());

                    EditorUtility.SetDirty(monoCullingGroup);
                }
            }
        }
    }
}
#endif