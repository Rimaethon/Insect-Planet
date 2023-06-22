using UnityEngine;

namespace AI_Behavior_System.Runtime {
    public abstract class DecoratorNode : Node {

        [SerializeReference]
        [HideInInspector] 
        public Node child;
    }
}
