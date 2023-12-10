// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public enum ReadyPose
    {
        LowReady,
        HighReady
    }

    public class LocomotionLayer : AnimLayer
    {
        [Header("Ready Poses")] [SerializeField]
        public LocRot highReadyPose;

        [SerializeField] public LocRot lowReadyPose;
        [SerializeField] private ReadyPose readyPoseType;
        [SerializeField] private float readyInterpSpeed;
        private float smoothReadyAlpha;
        private float readyPoseAlpha;

        [Header("Curve-Based Animation")] 
        [SerializeField] private float rotationScale = 1f;
        [SerializeField] private float translationScale = 1f;
        [SerializeField] private float smoothSpeed = 1f;

        // Curve-based animation
        private static readonly int RotX = Animator.StringToHash("IK_R_X");
        private static readonly int RotY = Animator.StringToHash("IK_R_Y");
        private static readonly int RotZ = Animator.StringToHash("IK_R_Z");
        private static readonly int LocX = Animator.StringToHash("IK_T_X");
        private static readonly int LocY = Animator.StringToHash("IK_T_Y");
        private static readonly int LocZ = Animator.StringToHash("IK_T_Z");

        [Header("Sprint")] [SerializeField, AnimCurveName(true)]
        protected string sprintPoseWeight;

        [SerializeField] protected AnimationCurve sprintBlendCurve = new(new Keyframe(0f, 0f));
        [SerializeField] protected LocRot sprintPose;

        private float smoothSprintLean;
        private LocRot smoothLoco = LocRot.identity;
        
        private LocRot curveAnimation = LocRot.identity;

        public void SetReadyWeight(float weight)
        {
            readyPoseAlpha = Mathf.Clamp01(weight);
        }

        
        public override void OnPreAnimUpdate()
        {
            base.OnPreAnimUpdate();
            smoothLayerAlpha *= 1f - core.animGraph.GetCurveValue(CurveLib.Curve_Overlay);
            core.animGraph.SetGraphWeight(1f - smoothLayerAlpha);
            core.ikRigData.weaponBoneWeight = GetCurveValue(CurveLib.Curve_WeaponBone);
        }

        public void UpdateCurveAnimation()
        {
            var animator = GetAnimator();
            
            Vector3 curveData;
            curveData.x = animator.GetFloat(RotX);
            curveData.y = animator.GetFloat(RotY);
            curveData.z = animator.GetFloat(RotZ);

            curveAnimation.rotation = Quaternion.Euler(curveData * rotationScale).normalized;
            
            curveData.x = animator.GetFloat(LocX);
            curveData.y = animator.GetFloat(LocY);
            curveData.z = animator.GetFloat(LocZ);
            curveData *= translationScale;

            curveAnimation.position = curveData;
        }

        public override void OnAnimUpdate()
        {
            ApplyReadyPose();
            ApplyLocomotion();
        }

        private void ApplyReadyPose()
        {
            var master = GetMasterPivot();

            float alpha = readyPoseAlpha * (1f - smoothLayerAlpha) * layerAlpha;
            smoothReadyAlpha = CoreToolkitLib.Glerp(smoothReadyAlpha, alpha, readyInterpSpeed);

            var finalPose = readyPoseType == ReadyPose.HighReady ? highReadyPose : lowReadyPose;

            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), master, finalPose.position, smoothReadyAlpha);
            CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, master, finalPose.rotation, smoothReadyAlpha);
        }

        private void ApplyLocomotion()
        {
            var master = GetMasterPivot();
            var mouseInput = GetCharData().deltaAimInput;

            smoothSprintLean = CoreToolkitLib.Glerp(smoothSprintLean, 4f * mouseInput.x, 3f);
            smoothSprintLean = Mathf.Clamp(smoothSprintLean, -15f, 15f);

            float alpha = sprintBlendCurve.Evaluate(smoothLayerAlpha);
            float locoAlpha = (1f - alpha) * layerAlpha;

            var leanVector = new Vector3(0f, smoothSprintLean, -smoothSprintLean);
            var sprintLean = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(leanVector), alpha);

            CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, GetPelvis(), sprintLean, 1f);

            UpdateCurveAnimation();
            CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, master, curveAnimation.rotation, locoAlpha);
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), master, curveAnimation.position, locoAlpha);

            smoothLoco = CoreToolkitLib.Glerp(smoothLoco,
                new LocRot(GetRigData().weaponBoneAdditive, false), smoothSpeed);

            GetMasterIK().Move(GetRootBone(), smoothLoco.position, locoAlpha);
            GetMasterIK().Rotate(GetRootBone().rotation, smoothLoco.rotation, locoAlpha);

            if (!string.IsNullOrEmpty(sprintPoseWeight))
            {
                alpha *= string.IsNullOrEmpty(sprintPoseWeight) ? 1f : GetAnimator().GetFloat(sprintPoseWeight);
            }

            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), master, sprintPose.position, alpha);
            CoreToolkitLib.RotateInBoneSpace(master.rotation, master, sprintPose.rotation, alpha);
        }
    }
}