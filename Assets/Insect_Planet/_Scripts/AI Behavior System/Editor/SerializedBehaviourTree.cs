using AI_Behavior_System.Runtime;
using UnityEditor;
using UnityEngine;

namespace AI_Behavior_System.Editor {

  
    public class SerializedBehaviourTree {

        // Wrapper serialized object for writing changes to the behaviour tree
        public readonly SerializedObject SerializedObject;
        public readonly BehaviourTree Tree;

        // Property names. These correspond to the variable names on the behaviour tree
        const string SPropRootNode = "rootNode";
        const string SPropNodes = "nodes";
        const string SPropBlackboard = "blackboard";
        const string SPropGuid = "guid";
        const string SPropChild = "child";
        const string SPropChildren = "children";
        const string SPropPosition = "position";
        const string SViewTransformPosition = "viewPosition";
        const string SViewTransformScale = "viewScale";

        public SerializedProperty RootNode {
            get {
                return SerializedObject.FindProperty(SPropRootNode);
            }
        }

        public SerializedProperty Nodes {
            get {
                return SerializedObject.FindProperty(SPropNodes);
            }
        }

        public SerializedProperty Blackboard {
            get {
                return SerializedObject.FindProperty(SPropBlackboard);
            }
        }

        // Start is called before the first frame update
        public SerializedBehaviourTree(BehaviourTree tree)
        {
            SerializedObject = new SerializedObject(tree);
            this.Tree = tree;
        }

        public void Save() {
            SerializedObject.ApplyModifiedProperties();
        }

        public SerializedProperty FindNode(SerializedProperty array, Node node) {
            for(int i = 0; i < array.arraySize; ++i) {
                var current = array.GetArrayElementAtIndex(i);
                if (current.FindPropertyRelative(SPropGuid).stringValue == node.guid) {
                    return current;
                }
            }
            return null;
        }

        public void SetViewTransform(Vector3 position, Vector3 scale) {
            SerializedObject.FindProperty(SViewTransformPosition).vector3Value = position;
            SerializedObject.FindProperty(SViewTransformScale).vector3Value = scale;
            SerializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void SetNodePosition(Node node, Vector2 position) {
            var nodeProp = FindNode(Nodes, node);
            nodeProp.FindPropertyRelative(SPropPosition).vector2Value = position;
            SerializedObject.ApplyModifiedProperties();
        }

        public void DeleteNode(SerializedProperty array, Node node) {
            for (int i = 0; i < array.arraySize; ++i) {
                var current = array.GetArrayElementAtIndex(i);
                if (current.FindPropertyRelative(SPropGuid).stringValue == node.guid) {
                    array.DeleteArrayElementAtIndex(i);
                    return;
                }
            }
        }

        public Node CreateNodeInstance(System.Type type) {
            Node node = System.Activator.CreateInstance(type) as Node;
            if (node != null)
            {
                node.guid = GUID.Generate().ToString();
                
            }
            return node;
        }

        SerializedProperty AppendArrayElement(SerializedProperty arrayProperty) {
            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            return arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
        }

        public Node CreateNode(System.Type type, Vector2 position) {

            Node node = CreateNodeInstance(type);
            node.position = position;

            SerializedProperty newNode = AppendArrayElement(Nodes);
            newNode.managedReferenceValue = node;

            SerializedObject.ApplyModifiedProperties();

            return node;
        }

        public void SetRootNode(RootNode node) {
            RootNode.managedReferenceValue = node;
            SerializedObject.ApplyModifiedProperties();
        }

        public void DeleteNode(Node node) {

            SerializedProperty nodesProperty = Nodes;

            for(int i = 0; i < nodesProperty.arraySize; ++i) {
                var prop = nodesProperty.GetArrayElementAtIndex(i);
                var guid = prop.FindPropertyRelative(SPropGuid).stringValue;
                DeleteNode(Nodes, node);
                SerializedObject.ApplyModifiedProperties();
            }
        }

        public void AddChild(Node parent, Node child) {
            
            var parentProperty = FindNode(Nodes, parent);

            // RootNode, Decorator node
            var childProperty = parentProperty.FindPropertyRelative(SPropChild);
            if (childProperty != null) {
                childProperty.managedReferenceValue = child;
                SerializedObject.ApplyModifiedProperties();
                return;
            }

            // Composite nodes
            var childrenProperty = parentProperty.FindPropertyRelative(SPropChildren);
            if (childrenProperty != null) {
                SerializedProperty newChild = AppendArrayElement(childrenProperty);
                newChild.managedReferenceValue = child;
                SerializedObject.ApplyModifiedProperties();
            }
        }

        public void RemoveChild(Node parent, Node child) {
            var parentProperty = FindNode(Nodes, parent);

            // RootNode, Decorator node
            var childProperty = parentProperty.FindPropertyRelative(SPropChild);
            if (childProperty != null) {
                childProperty.managedReferenceValue = null;
                SerializedObject.ApplyModifiedProperties();
                return;
            }

            // Composite nodes
            var childrenProperty = parentProperty.FindPropertyRelative(SPropChildren);
            if (childrenProperty != null) {
                DeleteNode(childrenProperty, child);
                SerializedObject.ApplyModifiedProperties();
            }
        }
    }
}
