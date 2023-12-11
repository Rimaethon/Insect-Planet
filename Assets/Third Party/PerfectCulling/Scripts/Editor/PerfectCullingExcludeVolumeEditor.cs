// Perfect Culling (C) 2021 Patrick König
//

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling
{
    [CustomEditor(typeof(PerfectCullingExcludeVolume))]
    public class PerfectCullingExcludeVolumeEditor : Editor
    {
        private readonly CustomHandle.ActualHandle<PerfectCullingExcludeVolume, float> m_handle =
            new CustomHandle.ActualHandle<PerfectCullingExcludeVolume, float>();
        
        SerializedObject so; // PerfectCullingExcludeVolume SO
        SerializedProperty volumeSize;
        SerializedProperty restrictToBehaviours;

        private void OnEnable()
        {
            PerfectCullingExcludeVolume excludeVolume = target as PerfectCullingExcludeVolume;

            so = new SerializedObject(excludeVolume);
            
            volumeSize = so.FindProperty("volumeSize");
            restrictToBehaviours = so.FindProperty("restrictToBehaviours");
        }
        

        public override void OnInspectorGUI()
        {  
            so.Update();
            {
                GUILayout.Label("Exclude Volume Configuration", EditorStyles.boldLabel);

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(volumeSize, new GUIContent("Volume Size"));
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // Indent this correctly so it doesn't overlap weirdly.
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(restrictToBehaviours,
                            new GUIContent("Restricted to Baking Behaviours"));
                    }
                }
            }
            so.ApplyModifiedProperties();
        }
        
        private void OnSceneGUI()
        {
            PerfectCullingExcludeVolume excludeVolume = target as PerfectCullingExcludeVolume;
            
            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "FrameSelected")
            {
                Event.current.commandName = "";
                Event.current.Use();

                UnityEditor.SceneView.lastActiveSceneView.Frame(excludeVolume.volumeExcludeBounds, false);
                
                return;
            }

            m_handle.DrawHandle(excludeVolume);
            
            // We draw the cube in OnDrawGizmosSelected in PerfectCullingExcludeVolume
        }
    }
}
#endif