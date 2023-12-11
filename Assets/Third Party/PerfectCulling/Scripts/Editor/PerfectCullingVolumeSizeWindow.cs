// Perfect Culling (C) 2021 Patrick König
//

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingVolumeSizeWindow : EditorWindow
    {
        public PerfectCullingVolume attachedVolume;
        
        private Bounds m_activeBounds;
        
        private void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        private void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
        }

        void OnSelectionChange()
        {
            m_activeBounds = default;
            
            List<Renderer> renderers = new List<Renderer>();
            
            foreach (var selectedGameObject in UnityEditor.Selection.gameObjects)
            {
                renderers.Clear();
                selectedGameObject.GetComponentsInChildren<Renderer>(renderers);

                foreach (Renderer r in renderers)
                {
                    if (m_activeBounds == default)
                    {
                        m_activeBounds = r.bounds;
                    }
                    else
                    {
                        m_activeBounds.Encapsulate(r.bounds);
                    }
                }
            }
            
            Repaint();
        }

        private void OnGUI()
        {
            GUILayout.Label("This tool assists you defining your volume size. It will be based on the bounds of the selected renderers.", EditorStyles.boldLabel);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Target:");
                attachedVolume =
                    (PerfectCullingVolume) EditorGUILayout.ObjectField(attachedVolume, typeof(PerfectCullingVolume),
                        true);

                GUILayout.Space(5);

                GUILayout.Label(
                    $"Selected renderers: {PerfectCullingUtil.FormatNumber(UnityEditor.Selection.gameObjects.Length)}");

                GUILayout.Space(5);

                Vector3 size = m_activeBounds.size;

                size.x = Mathf.Max(1, Mathf.CeilToInt(size.x));
                size.y = Mathf.Max(1, Mathf.CeilToInt(size.y));
                size.z = Mathf.Max(1, Mathf.CeilToInt(size.z));

                EditorGUILayout.Vector3IntField("Size: ", new Vector3Int((int) size.x, (int) size.y, (int) size.z));

                GUILayout.Space(5);

                if (GUILayout.Button($"Apply size {size.ToString()} to volume '{attachedVolume.name}'"))
                {
                    attachedVolume.volumeSize = size;

                    UnityEditor.EditorUtility.SetDirty(attachedVolume);
                }
            }
        }

        private void OnSceneGUI(SceneView scnView)
        {
            Handles.color = Color.cyan;
            
            Handles.DrawWireCube(m_activeBounds.center, m_activeBounds.size);
        }
    }
}

#endif