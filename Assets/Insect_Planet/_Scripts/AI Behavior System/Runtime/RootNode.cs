using UnityEngine;

namespace AI_Behavior_System.Runtime {

    [System.Serializable]
    public class RootNode : Node {

        [SerializeReference]
        [HideInInspector] 
        public Node child;

        protected override void OnStart() {

        }

        protected override void OnStop() {

        }

        protected override State OnUpdate() {

            return child.Update();
        }
    }
}