using AI_Behavior_System.Runtime;

namespace AI_Behavior_System.Composites
{
    [System.Serializable]
    public class ChooseTarget : CompositeNode {
 

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate()
        {
            
            return State.Success;
        }
    }
}
