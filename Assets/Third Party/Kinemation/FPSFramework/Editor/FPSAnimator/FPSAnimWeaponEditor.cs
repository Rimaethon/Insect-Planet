// Designed by KINEMATION, 2023

using System.IO;
using System.Reflection;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEditor;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.FPSAnimator
{
    [CustomEditor(typeof(FPSAnimWeapon), true)]
    [CanEditMultipleObjects]
    public class FPSAnimWeaponEditor : UnityEditor.Editor
    {
        private FPSAnimWeapon owner;
        private bool showAbstractProperties;

        private void OnEnable()
        {
            owner = (FPSAnimWeapon) target;
            // Load the foldout state from EditorPrefs
            showAbstractProperties = EditorPrefs.GetBool("MyAbstractClassEditor_showAbstractProperties", false);
        }

        private void OnDisable()
        {
            // Save the foldout state to EditorPrefs
            EditorPrefs.SetBool("MyAbstractClassEditor_showAbstractProperties", showAbstractProperties);
        }
        
        public override void OnInspectorGUI()
        {
            // Draw the foldout header for the abstract class properties
            
            GUIStyle foldoutHeaderStyle = new GUIStyle(EditorStyles.foldout);
            foldoutHeaderStyle.fontStyle = FontStyle.Bold;
            foldoutHeaderStyle.fontSize = 12;
            
            GUIContent foldoutHeaderContent = new GUIContent("FPSAnimWeapon Interface");
            Color previousColor = GUI.color;
            GUI.color = showAbstractProperties ? new Color(0.8f, 0.8f, 0.0f, 1.0f) : Color.yellow;
            GUI.color = previousColor;

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
            buttonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button(foldoutHeaderContent, buttonStyle))
            {
                showAbstractProperties = !showAbstractProperties;
            }

            if (showAbstractProperties)
            {
                // Draw a colored box to highlight the abstract class fields
                EditorGUILayout.BeginVertical(GUI.skin.box);

                // Draw the abstract class fields using reflection
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                FieldInfo[] fields = target.GetType().BaseType.GetFields(bindingFlags);

                foreach (FieldInfo field in fields)
                {
                    SerializedProperty property = serializedObject.FindProperty(field.Name);
                    if (property != null)
                    {
                        EditorGUILayout.PropertyField(property,
                            new GUIContent(ObjectNames.NicifyVariableName(field.Name)));
                    }
                }
                
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Setup Weapon"))
                {
                    owner.SetupWeapon();

                    if (PrefabUtility.IsPartOfPrefabInstance(owner.gameObject))
                    {
                        PrefabUtility.ApplyPrefabInstance(owner.gameObject, InteractionMode.AutomatedAction);
                    }
                }
                
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Save Weapon Position"))
                {
                    owner.SavePose();
                    
                    if (PrefabUtility.IsPartOfPrefabInstance(owner.gameObject))
                    {
                        // Apply the changes to the Prefab asset
                        PrefabUtility.ApplyPrefabInstance(owner.gameObject, InteractionMode.AutomatedAction);
                    }
                }

                // Reset the background color
                EditorGUILayout.EndVertical();
            }

            // Get all the abstract class field names to exclude them from the default inspector
            BindingFlags bindingFlagsForExclusion =
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            FieldInfo[] fieldsForExclusion = target.GetType().BaseType.GetFields(bindingFlagsForExclusion);
            string[] abstractFieldNames = new string[fieldsForExclusion.Length + 1];
            abstractFieldNames[0] = "m_Script";
            for (int i = 0; i < fieldsForExclusion.Length; i++)
            {
                abstractFieldNames[i + 1] = fieldsForExclusion[i].Name;
            }
                
            EditorGUILayout.Space();
            // Draw the default inspector for the derived class, excluding the abstract class fields
            DrawPropertiesExcluding(serializedObject, abstractFieldNames);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
