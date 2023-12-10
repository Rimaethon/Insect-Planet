// Designed by KINEMATION, 2023

using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public struct BoneTransform
    {
        public Transform bone;
        public Quaternion rotation;

        public BoneTransform(Transform boneRef)
        {
            bone = boneRef;
            rotation = Quaternion.identity;
        }

        public void CopyBone()
        {
            rotation = bone.localRotation;
        }
    }
    
    public class LeftHandIKLayer : AnimLayer
    {
        [Header("Left Hand IK Settings")]
        [AnimCurveName] [SerializeField] private string maskCurveName;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private AvatarMask leftHandMask;
        [SerializeField] private bool usePoseOverride = true;

        private LocRot _cache = LocRot.identity;
        private LocRot _final = LocRot.identity;
        
        private LocRot defaultLeftHand = LocRot.identity;
        private List<BoneTransform> leftHandChain = new List<BoneTransform>();

        public override void OnAnimStart()
        {
            if (leftHandMask == null)
            {
                Debug.LogWarning("LeftHandIKLayer: no mask for the left hand assigned!");
                return;
            }

            leftHandChain.Clear();
            for (int i = 1; i < leftHandMask.transformCount; i++)
            {
                if (leftHandMask.GetTransformActive(i))
                {
                    var t = transform.Find(leftHandMask.GetTransformPath(i));
                    leftHandChain.Add(new BoneTransform(t));
                }
            }
        }

        public override void OnPoseSampled()
        {
            if (!usePoseOverride)
            {
                return;
            }
            
            _cache = _final;

            if (leftHandMask == null) return;

            for (int i = 0; i < leftHandChain.Count; i++)
            {
                var bone = leftHandChain[i];
                bone.CopyBone();
                leftHandChain[i] = bone;
            }

            var rotOffset = GetGunAsset().rotationOffset;
            var gunBone = GetRigData().weaponBone;
            
            gunBone.rotation *= rotOffset;
            defaultLeftHand.position = gunBone.InverseTransformPoint(GetLeftHandIK().target.position);
            defaultLeftHand.rotation = Quaternion.Inverse(gunBone.rotation) * GetLeftHandIK().target.rotation;
            gunBone.rotation *= Quaternion.Inverse(rotOffset);
        }

        private void OverrideLeftHand(float weight)
        {
            weight = Mathf.Clamp01(weight);

            if (Mathf.Approximately(weight, 0f))
            {
                return;
            }
            
            foreach (var bone in leftHandChain)
            {
                bone.bone.localRotation = Quaternion.Slerp(bone.bone.localRotation, bone.rotation, weight);
            }
        }
        
        private LocRot basePoseT = LocRot.identity;
        private LocRot handTransform = LocRot.identity;

        public void UpdateHandTransforms()
        {
            basePoseT.position = GetMasterPivot().InverseTransformPoint(GetLeftHand().position) + GetPivotOffset();
            basePoseT.rotation =
                Quaternion.Inverse(Quaternion.Inverse(GetMasterPivot().rotation) * GetLeftHand().rotation);
            
            if (GetTransforms().leftHandTarget == null)
            {
                handTransform = defaultLeftHand;
            }
            else
            {
                var target = GetTransforms().leftHandTarget;
                handTransform = new LocRot(target.localPosition, target.localRotation);
            }
            
            handTransform.position -= basePoseT.position;
            handTransform.rotation *= basePoseT.rotation;
        }

        public override void OnAnimUpdate()
        {
            UpdateHandTransforms();
            
            float alpha = (1f - GetCurveValue(maskCurveName)) * (1f - smoothLayerAlpha) * layerAlpha;
            float progress = core.animGraph.GetPoseProgress();

            _final = CoreToolkitLib.Lerp(_cache, handTransform, progress);

            if (usePoseOverride)
            {
                OverrideLeftHand(alpha * (1f - core.animGraph.graphWeight));
            }
            
            GetLeftHandIK().Move(GetMasterPivot(), _final.position, alpha);
            GetLeftHandIK().Rotate(GetMasterPivot().rotation, _final.rotation, alpha);
        }
    }
}