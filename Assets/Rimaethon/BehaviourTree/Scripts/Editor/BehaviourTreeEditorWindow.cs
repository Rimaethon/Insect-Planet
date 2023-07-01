using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;

namespace TheKiwiCoder
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        public class Test : AssetModificationProcessor
        {
            private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
            {
                var wnd = GetWindow<BehaviourTreeEditorWindow>();
                wnd.ClearIfSelected(path);
                return AssetDeleteResult.DidNotDelete;
            }
        }

        private SerializedBehaviourTree serializer;
        private BehaviourTreeSettings settings;

        private BehaviourTreeView treeView;
        private InspectorView inspectorView;
        private BlackboardView blackboardView;
        private OverlayView overlayView;
        private ToolbarMenu toolbarMenu;
        private Label titleLabel;

        [MenuItem("TheKiwiCoder/BehaviourTreeEditor ...")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        public static void OpenWindow(BehaviourTree tree)
        {
            var wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
            wnd.SelectTree(tree);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow(Selection.activeObject as BehaviourTree);
                return true;
            }

            return false;
        }

        public void CreateGUI()
        {
            settings = BehaviourTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree = settings.behaviourTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = settings.behaviourTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            treeView = root.Q<BehaviourTreeView>();
            inspectorView = root.Q<InspectorView>();
            blackboardView = root.Q<BlackboardView>();
            toolbarMenu = root.Q<ToolbarMenu>();
            overlayView = root.Q<OverlayView>("OverlayView");
            titleLabel = root.Q<Label>("TitleLabel");

            // Toolbar assets menu
            toolbarMenu.RegisterCallback<MouseEnterEvent>((evt) =>
            {
                // Refresh the menu options just before it's opened (on mouse enter)
                toolbarMenu.menu.MenuItems().Clear();
                var behaviourTrees = EditorUtility.GetAssetPaths<BehaviourTree>();
                behaviourTrees.ForEach(path =>
                {
                    var fileName = System.IO.Path.GetFileName(path);
                    toolbarMenu.menu.AppendAction($"{fileName}", (a) =>
                    {
                        var tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
                        SelectTree(tree);
                    });
                });
                toolbarMenu.menu.AppendSeparator();
                toolbarMenu.menu.AppendAction("New Tree...", (a) => OnToolbarNewAsset());
            });

            // Overlay view
            treeView.OnNodeSelected = OnNodeSelectionChanged;
            overlayView.OnTreeSelected += SelectTree;
            Undo.undoRedoPerformed += OnUndoRedo;

            if (serializer == null)
                overlayView.Show();
            else
                SelectTree(serializer.tree);
        }

        private void OnUndoRedo()
        {
            if (serializer != null) treeView.PopulateView(serializer);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += OnSelectionChange;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.delayCall += OnSelectionChange;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject)
            {
                var runner = Selection.activeGameObject.GetComponent<BehaviourTreeRunner>();
                if (runner) SelectTree(runner.tree);
            }
        }

        private void SelectTree(BehaviourTree newTree)
        {
            if (!newTree)
            {
                ClearSelection();
                return;
            }

            serializer = new SerializedBehaviourTree(newTree);

            if (titleLabel != null)
            {
                var path = AssetDatabase.GetAssetPath(serializer.tree);
                if (path == "") path = serializer.tree.name;
                titleLabel.text = $"TreeView ({path})";
            }

            overlayView.Hide();
            treeView.PopulateView(serializer);
            blackboardView.Bind(serializer);
        }

        private void ClearSelection()
        {
            serializer = null;
            overlayView.Show();
            treeView.ClearView();
        }

        private void ClearIfSelected(string path)
        {
            if (AssetDatabase.GetAssetPath(serializer.tree) == path)
                // Need to delay because this is called from a will delete asset callback
                EditorApplication.delayCall += () => { SelectTree(null); };
        }

        private void OnNodeSelectionChanged(NodeView node)
        {
            inspectorView.UpdateSelection(serializer, node);
        }

        private void OnInspectorUpdate()
        {
            treeView?.UpdateNodeStates();
        }

        private void OnToolbarNewAsset()
        {
            var tree = EditorUtility.CreateNewTree("New Behaviour Tree", "Assets/");
            if (tree) SelectTree(tree);
        }
    }
}