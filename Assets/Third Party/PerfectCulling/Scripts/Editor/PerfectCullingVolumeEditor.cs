// Perfect Culling (C) 2021 Patrick König
//

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling
{
    [CustomEditor((typeof(PerfectCullingVolume)))]
    public class PerfectCullingVolumeEditor : Editor
    {
        private bool m_rendererFoldout;
        
        private SerializedObject so; // PerfectCullingVolume SO
        private SerializedProperty volumeSize;
        private SerializedProperty volumeBakeData;
        private SerializedProperty bakeCellSize;
        private SerializedProperty downsampleIterations;
        private SerializedProperty outOfBoundsBehaviour;
        private SerializedProperty searchNonEmptyCell;
        private SerializedProperty emptyCellBehaviour;
        private SerializedProperty additionalOccluders;
        private SerializedProperty visibilityLayer;

        private int m_bakeHash;
        
        private void OnEnable()
        {
            PerfectCullingVolume cullingVolume = target as PerfectCullingVolume;
            
            so = new SerializedObject(cullingVolume);
            
            volumeSize = so.FindProperty("volumeSize");
            volumeBakeData = so.FindProperty("volumeBakeData");
            bakeCellSize = so.FindProperty("bakeCellSize");
            downsampleIterations = so.FindProperty("mergeDownsampleIterations");
            outOfBoundsBehaviour = so.FindProperty("outOfBoundsBehaviour");
            searchNonEmptyCell = so.FindProperty("searchForNonEmptyCells");
            emptyCellBehaviour = so.FindProperty("emptyCellCullBehaviour");
            additionalOccluders = so.FindProperty("additionalOccluders");
            visibilityLayer = so.FindProperty("visibilityLayer");
            
            RefreshHash();
        }

        private void RefreshHash()
        {
            PerfectCullingVolume cullingVolume = target as PerfectCullingVolume;
            
            m_bakeHash = cullingVolume.GetBakeHash();
        }
        
        private readonly CustomHandle.ActualHandle<PerfectCullingVolume, int> m_handle =
            new CustomHandle.ActualHandle<PerfectCullingVolume, int>();

        private void OnSceneGUI()
        {
            PerfectCullingVolume cullingVolume = target as PerfectCullingVolume;

            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "FrameSelected")
            {
                Event.current.commandName = "";
                Event.current.Use();

                UnityEditor.SceneView.lastActiveSceneView.Frame(cullingVolume.volumeBakeBounds, false);
                
                return;
            }
            
            m_handle.DrawHandle(cullingVolume);

            // We draw the cube in OnDrawGizmosSelected in PerfectCullingVolume
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            {
                PerfectCullingVolume cullingVolume = target as PerfectCullingVolume;
                
                DrawUI(cullingVolume);
            }
            so.ApplyModifiedProperties();
        }

        void DrawUI(PerfectCullingVolume cullingVolume)
        {
            if (PerfectCullingSettings.Instance == null)
            {
                EditorGUILayout.HelpBox("Cannot find PerfectCullingSettings in Perfect Culling Resources folder.", MessageType.Error);
                
                return;
            }

            BakeSetup(cullingVolume);

            CurrentBake(cullingVolume);
            
            Visualization(cullingVolume);
        }

        void BakeSetup(PerfectCullingVolume cullingVolume)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {  
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("Baking Volume Configuration", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(outOfBoundsBehaviour, new GUIContent( "Camera Out Of Bounds Behaviour" ) );
                    EditorGUILayout.PropertyField(searchNonEmptyCell, new GUIContent( "Attempt to find non-empty cell" ) );
                    EditorGUILayout.PropertyField(emptyCellBehaviour, new GUIContent( "Empty Cell Behaviour" ) );
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUI.enabled = !Application.isPlaying;
                        EditorGUILayout.PropertyField(volumeSize, new GUIContent("Volume Size"));

                        if (GUILayout.Button("Size Utility", GUILayout.Width(100)))
                        {
                            var win = UnityEditor.EditorWindow.GetWindow<PerfectCullingVolumeSizeWindow>(true,
                                "Size Utility");
                            win.attachedVolume = cullingVolume;
                            win.Show();
                        }

                        GUI.enabled = true;
                    }
                    
                    GUI.enabled = !Application.isPlaying;
                    {
                        EditorGUILayout.PropertyField(bakeCellSize, new GUIContent("Cell Size"));
                        EditorGUILayout.PropertyField(downsampleIterations,
                            new GUIContent("Merge-Downsample Iterations"));
                    }
                    GUI.enabled = true;
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel("Visibility Layer");
                        visibilityLayer.intValue = (int)(PerfectCullingVisibilityLayer) EditorGUILayout.EnumFlagsField((PerfectCullingVisibilityLayer)visibilityLayer.intValue);
                    }
                    
                    if (downsampleIterations.intValue <= 0)
                    {
                        EditorGUILayout.HelpBox($"Consider at least one Merge-Downsample Iteration to avoid culling artifacts.", MessageType.Info);
                    }

                    if (cullingVolume.SamplingProviders.Count != 1)
                    {
                        GUILayout.Label("Custom Sampling Providers in use:", EditorStyles.boldLabel);
                        
                        foreach (var x in cullingVolume.SamplingProviders)
                        {
                            if (x.Name == DefaultActiveSamplingProvider.DefaultActiveSamplingProviderName)
                            {
                                continue;
                            }
                            
                            GUILayout.Label($"- {x.Name}");
                        }
                    }
                    
                    GUILayout.Space(10);
                    
                    if (cullingVolume.volumeSize.x % cullingVolume.bakeCellSize.x != 0)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.HelpBox(
                                $"Cell size for X: {cullingVolume.bakeCellSize.x} is not a divisor of {cullingVolume.volumeSize.x}, baking using this cell size will cause problems.",
                                MessageType.Error);

                            if (GUILayout.Button("Fix"))
                            {
                                cullingVolume.bakeCellSize.x = PerfectCullingUtil.FindValidDivisorCloseToUserProvided(cullingVolume.bakeCellSize.x, cullingVolume.volumeSize.x);
                            }
                        }
                    }
                
                    if (cullingVolume.volumeSize.y % cullingVolume.bakeCellSize.y != 0)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.HelpBox(
                                $"Cell size for Y: {cullingVolume.bakeCellSize.y} is not a divisor of {cullingVolume.volumeSize.y}, baking using this cell size will cause problems.",
                                MessageType.Error);

                            if (GUILayout.Button("Fix"))
                            {
                                cullingVolume.bakeCellSize.y = PerfectCullingUtil.FindValidDivisorCloseToUserProvided(cullingVolume.bakeCellSize.y, cullingVolume.volumeSize.y);
                            }
                        }
                    }
                
                    if (cullingVolume.volumeSize.z % cullingVolume.bakeCellSize.z != 0)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.HelpBox(
                                $"Cell size for Z: {cullingVolume.bakeCellSize.z} is not a divisor of {cullingVolume.volumeSize.z}, baking using this cell size will cause problems.",
                                MessageType.Error);

                            if (GUILayout.Button("Fix"))
                            {
                                cullingVolume.bakeCellSize.z = PerfectCullingUtil.FindValidDivisorCloseToUserProvided(cullingVolume.bakeCellSize.z, cullingVolume.volumeSize.z);
                            }
                        }
                    }
                }
  
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("Groups", EditorStyles.boldLabel);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // Indent this correctly so it doesn't overlap weirdly.
                        GUILayout.Space(10);
                        
                        m_rendererFoldout = EditorGUILayout.Foldout(m_rendererFoldout,
                            $"Bake groups ({cullingVolume.bakeGroups.Length})");
                    }
                    
                    if (m_rendererFoldout)
                    {
                        foreach (var cullingBakeGroup in cullingVolume.bakeGroups)
                        {
                            Rect rect = EditorGUILayout.BeginVertical();
                            GUILayout.Label($"Type: {cullingBakeGroup.groupType}");
                            foreach (var renderer in cullingBakeGroup.renderers)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (renderer == null)
                                    {
                                        EditorGUILayout.HelpBox("Invalid renderer", MessageType.Error);

                                        if (GUILayout.Button("X"))
                                        {
                                            cullingBakeGroup.renderers = cullingBakeGroup.renderers
                                                .Except(new Renderer[] {renderer}).ToArray();

                                            return;
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Label(" - " + renderer.name);

                                        if (GUILayout.Button("Select", GUILayout.Width(75)))
                                        {
                                            UnityEditor.Selection.activeObject = renderer;
                                        }

                                        if (GUILayout.Button("X", GUILayout.Width(25)))
                                        {
                                            cullingBakeGroup.renderers = cullingBakeGroup.renderers
                                                .Except(new Renderer[] {renderer}).ToArray();

                                            return;
                                        }
                                    }
                                }
                            }

                            if (GUILayout.Button("Remove group"))
                            {
                                cullingVolume.bakeGroups =
                                    cullingVolume.bakeGroups.Except(new PerfectCullingBakeGroup[] {cullingBakeGroup})
                                        .ToArray();

                                return;
                            }

                            EditorGUILayout.EndVertical();
                            GUI.Box(rect, GUIContent.none);
                            GUILayout.Space(5);
                        }
                    }

                    GUILayout.Space(10);

                    if (GUILayout.Button($"Clear bake groups"))
                    {
                        if (UnityEditor.EditorUtility.DisplayDialog("Are you sure?",
                            "This will clear the baked data thus requiring a rebake.\n\nThis step cannot be reverted.",
                            "OK", "Cancel"))
                        {
                            cullingVolume.bakeGroups = System.Array.Empty<PerfectCullingBakeGroup>();

                            EditorUtility.SetDirty(cullingVolume);
                        }
                    }
                    
                    GUILayout.Space(10);
                
                    if (GUILayout.Button("Open Renderer Selection Tool"))
                    {
                        var win = UnityEditor.EditorWindow.GetWindow<PerfectCullingRendererSelectionWindow>(true, "Renderer selection");
                        win.attachedBakingBehaviour = cullingVolume;
                        win.Show();
                    }
                }
                
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {    
                    GUILayout.Label("Other", EditorStyles.boldLabel);
                    
                    GUILayout.Label("Allows to specifiy renderers that are occluders but not occludees.");
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // Indent this correctly so it doesn't overlap weirdly.
                        GUILayout.Space(10);

                        EditorGUILayout.PropertyField(additionalOccluders, new GUIContent("Additional Occluders"));
                    }
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("Bake", EditorStyles.boldLabel);
                    GUILayout.Label($"Cells: {PerfectCullingUtil.FormatNumber(cullingVolume.CellCount)}");

                    Vector3 startBakeSize = cullingVolume.bakeCellSize;
                    for (int i = 0; i < cullingVolume.mergeDownsampleIterations; ++i)
                    {
                        startBakeSize *= 2;
                        
                        int cellCount = PerfectCullingMath.CalculateNumberOfCells(cullingVolume.volumeSize, startBakeSize);
                        
                        GUILayout.Label($"-> Merge-Downsample iteration {i + 1}: {PerfectCullingUtil.FormatNumber(cellCount)} cells");

                        if (cellCount <= 1)
                        {
                            break;
                        }
                    }

                    float bakeSpeed = PerfectCullingSettings.Instance.bakeAverageSamplingSpeedMs;

                    GUILayout.Label($"ETA: {PerfectCullingUtil.FormatSeconds(((double)cullingVolume.CellCount * bakeSpeed) * 0.001f)} @ {bakeSpeed} ms/sample");
                    
                    //if (cullingVolume.SamplingProviders.Count >= 1)
                    {
                        if (GUILayout.Button("Calculate cells and ETA after cell exclusion"))
                        {
                            int totalCells = cullingVolume.CellCount;

                            List<Vector3> allPositions = cullingVolume.GetSamplingPositions(Space.World);

                            int activeCells = 0;
                            
                            cullingVolume.InitializeAllSamplingProviders();
                            
                            foreach (Vector3 pos in allPositions)
                            {
                                activeCells += cullingVolume.SamplingProvidersIsPositionActive(pos) ? 1 : 0;
                            }

                            float exclusionFactor = (activeCells / ((float) totalCells));
                            int finalCellCount = Mathf.CeilToInt(exclusionFactor * PerfectCullingMath.CalculateNumberOfCells(cullingVolume.volumeSize, startBakeSize));
                            EditorUtility.DisplayDialog("ETA", $" * Total cells: {PerfectCullingUtil.FormatNumber(totalCells)}\n * Active cells: {PerfectCullingUtil.FormatNumber(activeCells)} ({exclusionFactor * 100f}%)\n * After Merge-Downsampling: ~{PerfectCullingUtil.FormatNumber(finalCellCount)}\n\nETA: {PerfectCullingUtil.FormatSeconds(((double)activeCells * bakeSpeed) * 0.001f)} @ {bakeSpeed} ms/sample", "OK");
                        }
                    }

                    bool canBake = !Application.isPlaying && cullingVolume.gameObject.scene != default && !UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(cullingVolume.gameObject.scene) && cullingVolume.bakeGroups.Length > 0;
                    
                    GUI.enabled = canBake;
                    {
                        if (GUILayout.Button($"Bake"))
                        { 
                            if (cullingVolume.volumeBakeData == null)
                            {
                                CreateBakeData(cullingVolume);
                            }
                            
                            PerfectCullingBakingManager.BakeNow(cullingVolume);
                        }
                    }
                    GUI.enabled = true;

                    if (cullingVolume.bakeGroups.Length <= 0)
                    {
                        EditorGUILayout.HelpBox("Nothing to bake. Select \"Open Renderer Selection Tool\" to add renderers.", MessageType.Warning);
                    }
                }
            }
        }

        void CreateBakeData(PerfectCullingVolume cullingVolume)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create bake data",
                $"Occlusion_{UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name}.asset",
                "asset",
                "Select destination for bake data");

            if (path.Length == 0)
            {
                return;
            }

            cullingVolume.volumeBakeData = ScriptableObject.CreateInstance<PerfectCullingVolumeBakeData>();
                            
            UnityEditor.EditorUtility.SetDirty(cullingVolume);
                            
            UnityEditor.AssetDatabase.CreateAsset(cullingVolume.volumeBakeData, path);
            UnityEditor.AssetDatabase.SaveAssets();
        }
        
        void CurrentBake(PerfectCullingVolume cullingVolume)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Current bake", EditorStyles.boldLabel);

                if (cullingVolume.volumeBakeData != null)
                {
                    EditorGUILayout.PropertyField(volumeBakeData, new GUIContent("Baked Data"));

                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(cullingVolume.volumeBakeData);

                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        if (cullingVolume.volumeBakeData.data != null)
                        {
                            GUILayout.Label("Baked cells: " +
                                            PerfectCullingUtil.FormatNumber(
                                                cullingVolume.volumeBakeData.data.Length));
                            GUILayout.Label("Baked cell size: " + cullingVolume.volumeBakeData.cellSize.ToString());

                            if (PerfectCullingEditorUtil.TryGetAssetBakeSize(cullingVolume.volumeBakeData,
                                out float bakeSizeMb))
                            {

                                GUILayout.Label($"Current bake size: {bakeSizeMb} mb(s)");
                            }
                        }

                        if (!cullingVolume.volumeBakeData.bakeCompleted && !PerfectCullingBakingManager.IsBaking)
                        {
                            EditorGUILayout.HelpBox(
                                $"This bake was not completed and might be corrupted. Please consider to bake again.",
                                MessageType.Error);
                        }

                        if (cullingVolume.bakeGroups.Length != cullingVolume.volumeBakeData.numberOfGroups)
                        {
                            EditorGUILayout.HelpBox($"The number of bake groups has changed since last bake. Rebake might be required.", MessageType.Warning);
                        }

                        if (cullingVolume.volumeBakeData.bakeHash != m_bakeHash)
                        {
                            EditorGUILayout.HelpBox($"Hash doesn't match. Rebake might be required.", MessageType.Warning);
                        }
                    }
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(volumeBakeData, new GUIContent("Baked Data"));

                        if (GUILayout.Button("Create bake data", GUILayout.Width(150)))
                        {
                            CreateBakeData(cullingVolume);
                        }
                    }
                }
            }
        }

        void Visualization(PerfectCullingVolume cullingVolume)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Visualization", EditorStyles.boldLabel);

                /*
                if (GUILayout.Button($"Visualization: {(cullingVolume.Visualize ? "ON" : "OFF")}"))
                {
                    cullingVolume.Visualize = !cullingVolume.Visualize;

                    UnityEditor.SceneView.RepaintAll();
                }*/
                
                EditorGUI.BeginChangeCheck();

                bool volumeVisualize = GUILayout.Toggle(cullingVolume.VisualizeProbes , " Visualize probes");
                    
                if (EditorGUI.EndChangeCheck())
                {
                    cullingVolume.VisualizeProbes  = volumeVisualize;
                    
                    UnityEditor.SceneView.RepaintAll();
                }
                
                EditorGUI.BeginChangeCheck();
                
                bool volumeGridCells = GUILayout.Toggle(cullingVolume.VisualizeGridCells , " Visualize grid cells");
                    
                if (EditorGUI.EndChangeCheck())
                {
                    cullingVolume.VisualizeGridCells  = volumeGridCells;
                    
                    UnityEditor.SceneView.RepaintAll();
                }

                if (cullingVolume.VisualizeProbes || cullingVolume.VisualizeGridCells)
                {
                    cullingVolume.VisualizeHitLines = GUILayout.Toggle(cullingVolume.VisualizeHitLines, $" Draw lines towards visible renderers");
                }
                
                GUILayout.Space(10);

                GUI.enabled = Application.isPlaying;
                {
                    if (GUILayout.Button("Replace materials in scene with bake color material"))
                    {
                        PerfectCullingSceneColor sceneColor = new PerfectCullingSceneColor(cullingVolume.bakeGroups, null);
                    }
                }
                GUI.enabled = true;

                if (!Application.isPlaying)
                {
                    GUILayout.Space(5);
                    
                    EditorGUILayout.HelpBox($"Some functionality is only available in Play Mode!", MessageType.Info);
                }
            }
        }
    }
}
#endif