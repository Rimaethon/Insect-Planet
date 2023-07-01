using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        // The main behaviour tree asset
        public BehaviourTree tree;

        // Storage container object to hold game object subsystems
        private Context context;

        // Start is called before the first frame update
        private void Start()
        {
            context = CreateBehaviourTreeContext();
            tree = tree.Clone();
            tree.Bind(context);
        }

        // Update is called once per frame
        private void Update()
        {
            if (tree) tree.Update();
        }

        private Context CreateBehaviourTreeContext()
        {
            return Context.CreateFromGameObject(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (!tree) return;

            BehaviourTree.Traverse(tree.rootNode, (n) =>
            {
                if (n.drawGizmos) n.OnDrawGizmos();
            });
        }
    }
}