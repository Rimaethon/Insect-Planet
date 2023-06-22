using AI_Behavior_System.Runtime;
using UnityEngine;

namespace AI_Behavior_System.Actions {
    [System.Serializable]
    public class Log : ActionNode
    {
        public string message;

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            Debug.Log($"{message}");
            return State.Success;
        }
    }
}
