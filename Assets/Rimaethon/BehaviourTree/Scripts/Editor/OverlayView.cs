using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace TheKiwiCoder
{
    public class OverlayView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<OverlayView, UxmlTraits>
        {
        }

        public System.Action<BehaviourTree> OnTreeSelected;

        private Button openButton;
        private Button createButton;
        private DropdownField assetSelector;
        private TextField treeNameField;
        private TextField locationPathField;

        public void Show()
        {
            // Hidden in UIBuilder while editing..
            style.visibility = Visibility.Visible;

            // Configure fields
            openButton = this.Q<Button>("OpenButton");
            assetSelector = this.Q<DropdownField>();
            createButton = this.Q<Button>("CreateButton");
            treeNameField = this.Q<TextField>("TreeName");
            locationPathField = this.Q<TextField>("LocationPath");

            // Configure asset selection dropdown menu
            var behaviourTrees = EditorUtility.GetAssetPaths<BehaviourTree>();
            assetSelector.choices = new List<string>();
            behaviourTrees.ForEach(treePath => { assetSelector.choices.Add(ToMenuFormat(treePath)); });

            // Configure open asset button
            openButton.clicked -= OnOpenAsset;
            openButton.clicked += OnOpenAsset;

            // Configure create asset button
            createButton.clicked -= OnCreateAsset;
            createButton.clicked += OnCreateAsset;
        }

        public void Hide()
        {
            style.visibility = Visibility.Hidden;
        }

        public string ToMenuFormat(string one)
        {
            // Using the slash creates submenus...
            return one.Replace("/", "|");
        }

        public string ToAssetFormat(string one)
        {
            // Using the slash creates submenus...
            return one.Replace("|", "/");
        }

        private void OnOpenAsset()
        {
            var path = ToAssetFormat(assetSelector.text);
            var tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
            if (tree)
            {
                TreeSelected(tree);
                style.visibility = Visibility.Hidden;
            }
        }

        private void OnCreateAsset()
        {
            var tree = EditorUtility.CreateNewTree(treeNameField.text, locationPathField.text);
            if (tree)
            {
                TreeSelected(tree);
                style.visibility = Visibility.Hidden;
            }
        }

        private void TreeSelected(BehaviourTree tree)
        {
            OnTreeSelected.Invoke(tree);
        }
    }
}