using AI_Behavior_System.Runtime;
using UnityEngine;

namespace AI_Behavior_System.Actions {

    [System.Serializable]
    public class Wait : ActionNode {

        public float duration = 3;
        float startTime;

        protected override void OnStart() {
            startTime = Time.time;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            
            float timeRemaining = Time.time - startTime;
            if (timeRemaining > duration) {
                return State.Success;
            }
            return State.Running;
        }
    }
}
