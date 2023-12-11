// Perfect Culling (C) 2021 Patrick König
//

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    [CustomEditor(typeof(PerfectCullingColorTable))]
    public class PerfectCullingColorTableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This asset stores pre-computed unique color information that is critical for the baking process.\nNothing useful to see here, I'm afraid.", MessageType.Info);
        }
    }
}
#endif