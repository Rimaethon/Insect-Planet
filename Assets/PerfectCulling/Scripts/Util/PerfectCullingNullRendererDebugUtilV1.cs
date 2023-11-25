// Perfect Culling (C) 2022 Patrick König
//

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    [RequireComponent(typeof(PerfectCullingVolume))]
    public class PerfectCullingNullRendererDebugUtilV1 : MonoBehaviour
    {
        private class DebugData
        {
            public string Name;
            public string ParentName;
        }
        
        private readonly List<DebugData> m_names = new List<DebugData>();

        private void Start()
        {
            foreach (PerfectCullingBakeGroup group in GetVolume().bakeGroups)
            {
                foreach (Renderer r in group.renderers)
                {
                    if (r == null)
                    {
                        m_names.Add(new DebugData()
                        {
                            Name = "<NULL>",
                            ParentName = "<NULL>",
                        });
                    }
                    else
                    {
                        string parentName = r.transform.parent != null ? r.transform.parent.name : "<NULL>";
                        
                        m_names.Add(new DebugData()
                        {
                            Name = r.name,
                            ParentName = parentName,
                        });
                    }
                }
            }
        }

        public void ScanForNull()
        {
            int index = 0;
            int groupIndex = 0;
            
            foreach (PerfectCullingBakeGroup group in GetVolume().bakeGroups)
            {
                foreach (Renderer r in group.renderers)
                {
                    if (r == null)
                    {
                        UnityEngine.Debug.Log($"Detected null renderer!!! Name: '{m_names[index].Name}', Parent: '{m_names[index].ParentName}', Group Index: {groupIndex}, Index: {index}");
                    }
                    
                    ++index;
                }

                ++groupIndex;
            }
        }

        PerfectCullingVolume GetVolume() => GetComponent<PerfectCullingVolume>();
    }

    [CustomEditor(typeof(PerfectCullingNullRendererDebugUtilV1))]
    public class PerfectCullingNullRendererDebugUtilEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            PerfectCullingNullRendererDebugUtilV1 utilV1 = target as PerfectCullingNullRendererDebugUtilV1;

            if (utilV1 == null)
            {
                return;
            }

            if (GUILayout.Button("Scan"))
            {
                utilV1.ScanForNull();
            }
        }
    }
}
#endif