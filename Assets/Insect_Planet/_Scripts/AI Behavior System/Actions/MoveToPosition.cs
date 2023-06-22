using AI_Behavior_System.Runtime;

namespace AI_Behavior_System.Actions
{
    public class MoveToPosition : ActionNode {
        private float speed = 5;
        private float stoppingDistance = 0.1f;
        private bool updateRotation = true;
        private float acceleration = 40.0f;
        public float tolerance = 1.0f;

        

        protected override void OnStart()
        {
            context.agent.stoppingDistance = stoppingDistance;
            context.agent.speed = speed;
            context.agent.updateRotation = updateRotation;
            context.agent.acceleration = acceleration;
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
           
            return State.Running;
        }
    }
}
