// Designed by KINEMATION, 2023

using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Components
{
    public class ModifyCurve : StateMachineBehaviour
    {
        [SerializeField] private string paramName;
        [SerializeField] private float paramTargetValue;

        private int paramId;
        private float paramStartValue;
        private bool isInitialized;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!isInitialized)
            {
                paramId = Animator.StringToHash(paramName);
                isInitialized = true;
            }
        
            paramStartValue = animator.GetFloat(paramId);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            int nextHash = animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash;
            if (nextHash != stateInfo.fullPathHash && nextHash != 0)
            {
                return;
            }
        
            float alpha = 0f;
            if (animator.IsInTransition(layerIndex))
            {
                alpha = animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime;
            }
            else
            {
                alpha = 1f;
            }
        
            animator.SetFloat(paramId, Mathf.Lerp(paramStartValue, paramTargetValue, alpha));
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }
    }
}
