// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Layers;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Layers
{
    [CustomEditor(typeof(LookLayer), true)]
    public class LookLayerEditor : UnityEditor.Editor
    {
        private string[] tabs = {"Blending", "Aim Offset", "Leaning"};
        private int selectedTab;
        
        private SerializedProperty runInEditor;
        private SerializedProperty drawDebugInfo;
        private SerializedProperty layerAlpha;
        private SerializedProperty lerpSpeed;
        private SerializedProperty handsAlpha;
        private SerializedProperty handsLerp;
        private SerializedProperty pelvisAlpha;
        private SerializedProperty pelvisLerp;
        
        private SerializedProperty pelvisOffset;
        private SerializedProperty lookUpOffset;
        private SerializedProperty lookRightOffset;
        private SerializedProperty enableAutoDistribution;
        private SerializedProperty aimOffsetTable;
        private SerializedProperty aimUp;
        private SerializedProperty aimRight;
        private SerializedProperty smoothAim;
        
        private SerializedProperty leanDirection;
        private SerializedProperty leanAmount;
        private SerializedProperty leanSpeed;
        
        private SerializedProperty detectZeroFrames;
        private SerializedProperty checkZeroFootIK;
        private SerializedProperty useRightOffset;
        
        private SerializedProperty curveName;
        
        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            
            runInEditor = serializedObject.FindProperty("runInEditor");
            drawDebugInfo = serializedObject.FindProperty("drawDebugInfo");
            layerAlpha = serializedObject.FindProperty("layerAlpha");
            lerpSpeed = serializedObject.FindProperty("lerpSpeed");
            handsAlpha = serializedObject.FindProperty("handsLayerAlpha");
            handsLerp = serializedObject.FindProperty("handsLerpSpeed");
            pelvisAlpha = serializedObject.FindProperty("pelvisLayerAlpha");
            pelvisLerp = serializedObject.FindProperty("pelvisLerpSpeed");

            pelvisOffset = serializedObject.FindProperty("pelvisOffset");
            lookUpOffset = serializedObject.FindProperty("lookUpOffset");
            lookRightOffset = serializedObject.FindProperty("lookRightOffset");
            enableAutoDistribution = serializedObject.FindProperty("autoDistribution");
            aimOffsetTable = serializedObject.FindProperty("aimOffsetTable");
            aimUp = serializedObject.FindProperty("aimUp");
            aimRight = serializedObject.FindProperty("aimRight");
            smoothAim = serializedObject.FindProperty("smoothAim");

            leanDirection = serializedObject.FindProperty("leanDirection");
            leanAmount = serializedObject.FindProperty("leanAmount");
            leanSpeed = serializedObject.FindProperty("leanSpeed");

            detectZeroFrames = serializedObject.FindProperty("detectZeroFrames");
            checkZeroFootIK = serializedObject.FindProperty("checkZeroFootIK");
            useRightOffset = serializedObject.FindProperty("useRightOffset");
            
            curveName = serializedObject.FindProperty("curveName");
        }

        private void DrawBlendingTab()
        {
            EditorGUILayout.PropertyField(curveName);
            EditorGUILayout.PropertyField(layerAlpha);
            EditorGUILayout.PropertyField(lerpSpeed);
            EditorGUILayout.PropertyField(handsAlpha);
            EditorGUILayout.PropertyField(handsLerp);
            EditorGUILayout.PropertyField(pelvisAlpha);
            EditorGUILayout.PropertyField(pelvisLerp);
        }

        private void SaveTable()
        {
            var table = (target as LookLayer)?.SaveTable();

            if (aimOffsetTable.objectReferenceValue == null)
            {
                string path = "Assets/AO_NewTable.asset";
                string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(table, uniquePath);
            }
     
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
        }
        
        private void DrawOffsetTab()
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.PropertyField(pelvisOffset);
            EditorGUILayout.PropertyField(enableAutoDistribution);

            EditorGUILayout.BeginHorizontal();

            // Draw the label
            EditorGUILayout.LabelField("Aim Offset Table", GUILayout.Width(EditorGUIUtility.labelWidth));

            var propertyRect = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            float width = propertyRect.width * 0.5f;
            propertyRect.width = width;
            
            // Draw the property reference without the label
            EditorGUI.PropertyField(propertyRect, aimOffsetTable, GUIContent.none);

            propertyRect.x = propertyRect.xMax;
            propertyRect.width = width;
            if (GUI.Button(propertyRect, "Save table"))
            {
                SaveTable();
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(lookUpOffset);
            EditorGUILayout.PropertyField(lookRightOffset);
            EditorGUILayout.PropertyField(aimUp);
            EditorGUILayout.PropertyField(aimRight);
            EditorGUILayout.PropertyField(smoothAim);

            EditorGUILayout.EndVertical();
        }
        
        private void DrawLeanTab()
        {
            EditorGUILayout.PropertyField(leanDirection);
            EditorGUILayout.PropertyField(leanAmount);
            EditorGUILayout.PropertyField(leanSpeed);
        }
        
        private void DrawDefault()
        {
            EditorGUILayout.PropertyField(detectZeroFrames);
            EditorGUILayout.PropertyField(checkZeroFootIK);
            EditorGUILayout.PropertyField(useRightOffset);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(runInEditor);
            EditorGUILayout.PropertyField(drawDebugInfo);
            GUILayout.BeginVertical();
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            GUILayout.EndVertical();
            switch (selectedTab)
            {
                case 0:
                    DrawBlendingTab();
                    break;
                case 1:
                    DrawOffsetTab();
                    break;
                case 2:
                    DrawLeanTab(); ;
                    break;
            }

            DrawDefault();
            serializedObject.ApplyModifiedProperties();
        }
    }
}