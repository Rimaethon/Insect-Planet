// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class AdsLayer : AnimLayer
    {
        [Header("SightsAligner")] [SerializeField]
        private EaseMode adsEaseMode = new EaseMode(new[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 0f)
        });

        [SerializeField] private EaseMode pointAimEaseMode = new EaseMode(new[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 0f)
        });
        
        [SerializeField] [Bone] protected bool usePivotAdjustment = false;
        [SerializeField] [Bone] protected Transform aimTarget;

        [SerializeField] private LocRot crouchPose;
        [SerializeField] [AnimCurveName(true)] private string crouchPoseCurve;

        protected bool bAds;
        protected float adsProgress;
        
        protected bool bPointAim;
        protected float pointAimProgress;
        
        protected float adsWeight;
        protected float pointAimWeight;
        
        protected LocRot interpAimPoint;
        protected LocRot viewOffsetCache;
        protected LocRot viewOffset;
        
        protected LocRot weaponBoneMS = LocRot.identity;
        protected Vector3 aimTargetMS = Vector3.zero;
        
        public void SetAds(bool bAiming)
        {
            bAds = bAiming;
            interpAimPoint = bAds ? GetAdsOffset() : interpAimPoint;
        }
        
        public void SetPointAim(bool bAiming)
        {
            bPointAim = bAiming;
        }

        public override void OnPoseSampled()
        {
            var rotOffset = GetGunAsset().rotationOffset;
            
            weaponBoneMS = new LocRot(GetRigData().weaponBone);
            weaponBoneMS.rotation *= rotOffset;
            weaponBoneMS = weaponBoneMS.ToSpace(GetRootBone());

            aimTargetMS = new LocRot(aimTarget).ToSpace(GetRootBone()).position;
        }

        public override void OnAnimUpdate()
        {
            Vector3 baseLoc = GetMasterPivot().position;
            Quaternion baseRot = GetMasterPivot().rotation;

            ApplyCrouchPose();
            ApplyPointAiming();

            if (usePivotAdjustment)
            {
                var aimPoint = GetPivotPoint().InverseTransformPoint(GetAimPoint().position);
                aimPoint.z = 0f;
                core.ikRigData.adsPivotOffset = Vector3.Lerp(Vector3.zero, aimPoint, adsWeight);
            }
            
            ApplyAiming();
            
            Vector3 postLoc = GetMasterPivot().position;
            Quaternion postRot = GetMasterPivot().rotation;

            GetMasterPivot().position = Vector3.Lerp(baseLoc, postLoc, smoothLayerAlpha);
            GetMasterPivot().rotation = Quaternion.Slerp(baseRot, postRot, smoothLayerAlpha);
        }
        
        protected void UpdateAimWeights(float adsRate = 1f, float pointAimRate = 1f)
        {
            adsWeight = CurveLib.Ease(0f, 1f, adsProgress, adsEaseMode);
            pointAimWeight = CurveLib.Ease(0f, 1f, pointAimProgress, pointAimEaseMode);
            
            adsProgress += Time.deltaTime * (bAds ? adsRate : -adsRate);
            pointAimProgress += Time.deltaTime * (bPointAim ? pointAimRate : -pointAimRate);

            adsProgress = Mathf.Clamp(adsProgress, 0f, 1f);
            pointAimProgress = Mathf.Clamp(pointAimProgress, 0f, 1f);
        }

        protected LocRot GetAdsOffset()
        {
            LocRot adsOffset = new LocRot(Vector3.zero, Quaternion.identity);

            if (GetAimPoint() != null)
            {
                adsOffset.rotation = Quaternion.Inverse(GetPivotPoint().rotation) * GetAimPoint().rotation;
                adsOffset.position = -GetPivotPoint().InverseTransformPoint(GetAimPoint().position);
            }

            return adsOffset;
        }

        protected void BlendAiming(Vector3 addAimLoc, Quaternion addAimRot)
        {
            // Convert to root bone space
            Vector3 outPos = GetRootBone().InverseTransformPoint(GetMasterPivot().position);
            addAimLoc = GetRootBone().InverseTransformPoint(addAimLoc);

            var root = GetRootBone().rotation;
            var invRoot = Quaternion.Inverse(GetRootBone().rotation);
            
            // Retrieve the blending values
            var aimLayerAlphaLoc = GetGunAsset().adsData.adsTranslationBlend;
            var aimLayerAlphaRot = GetGunAsset().adsData.adsRotationBlend;

            // Blend translation
            outPos.x = Mathf.Lerp(outPos.x, addAimLoc.x, aimLayerAlphaLoc.x);
            outPos.y = Mathf.Lerp(outPos.y, addAimLoc.y, aimLayerAlphaLoc.y);
            outPos.z = Mathf.Lerp(outPos.z, addAimLoc.z, aimLayerAlphaLoc.z);
            
            // Convert to root bone space
            Vector3 eulerAim = CoreToolkitLib.ToEuler(invRoot * GetMasterPivot().rotation);
            Vector3 eulerAddAimRot = CoreToolkitLib.ToEuler(invRoot * addAimRot);
            
            // Blend rotation
            eulerAim.x = Mathf.Lerp(eulerAim.x, eulerAddAimRot.x, aimLayerAlphaRot.x);
            eulerAim.y = Mathf.Lerp(eulerAim.y, eulerAddAimRot.y, aimLayerAlphaRot.y);
            eulerAim.z = Mathf.Lerp(eulerAim.z, eulerAddAimRot.z, aimLayerAlphaRot.z);
            
            GetMasterPivot().rotation = root * Quaternion.Euler(eulerAim);
            GetMasterPivot().position = GetRootBone().TransformPoint(outPos);
        }

        protected virtual void ApplyAiming()
        {
            var aimData = GetGunAsset().adsData;

            float aimSpeed = GetGunAsset() != null ? GetGunAsset().adsData.aimSpeed : aimData.aimSpeed;
            float pointAimSpeed = GetGunAsset() != null ? GetGunAsset().adsData.pointAimSpeed : aimData.pointAimSpeed;
            float changeSightSpeed = GetGunAsset() != null ? GetGunAsset().adsData.changeSightSpeed : aimData.changeSightSpeed;
            
            // Base Animation layer
            
            LocRot defaultPose = new LocRot(GetMasterPivot());
            ApplyHandsOffset();
            LocRot basePose = new LocRot(GetMasterPivot());
            
            GetMasterPivot().position = defaultPose.position;
            GetMasterPivot().rotation = defaultPose.rotation;

            UpdateAimWeights(aimSpeed, pointAimSpeed);

            interpAimPoint = CoreToolkitLib.Glerp(interpAimPoint, GetAdsOffset(), changeSightSpeed);
            
            ApplyAdditiveAim();
            
            Vector3 addAimLoc = GetMasterPivot().position;
            Quaternion addAimRot = GetMasterPivot().rotation;

            GetMasterPivot().position = basePose.position;
            GetMasterPivot().rotation = basePose.rotation;
            ApplyAbsAim();

            // Blend between Absolute and Additive
            BlendAiming(addAimLoc, addAimRot);

            float aimWeight = Mathf.Clamp01(adsWeight - pointAimWeight);
            
            // Blend Between Non-Aiming and Aiming
            GetMasterPivot().position = Vector3.Lerp(basePose.position, GetMasterPivot().position, aimWeight);
            GetMasterPivot().rotation = Quaternion.Slerp(basePose.rotation, GetMasterPivot().rotation, aimWeight);
        }

        protected void ApplyCrouchPose()
        {
            float poseAlpha = GetAnimator().GetFloat(crouchPoseCurve) * (1f - adsWeight);
            GetMasterIK().Move(GetRootBone(), crouchPose.position, poseAlpha);
            GetMasterIK().Rotate(GetRootBone().rotation, crouchPose.rotation, poseAlpha);
        }

        protected virtual void ApplyPointAiming()
        {
            var pointAimOffset = GetGunAsset().adsData.pointAimOffset;
            
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), GetMasterPivot(),
                pointAimOffset.position, pointAimWeight);
            CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, GetMasterPivot(),
                pointAimOffset.rotation, pointAimWeight);
        }

        protected virtual void ApplyHandsOffset()
        {
            float progress = core.animGraph.GetPoseProgress();
            if (Mathf.Approximately(progress, 0f))
            {
                viewOffsetCache = viewOffset;
            }

            var targetViewOffset = GetGunAsset().viewOffset;
            viewOffset = CoreToolkitLib.Lerp(viewOffsetCache, targetViewOffset, progress);
            
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(),GetMasterPivot(), 
                viewOffset.position, 1f);
            CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, GetMasterPivot(), 
                viewOffset.rotation, 1f);
        }

        protected void ApplyAdditiveAim()
        {
            LocRot cachedIK = new LocRot(GetMasterPivot());
            
            // Apply the base pose
            GetMasterPivot().position = GetRootBone().TransformPoint(weaponBoneMS.position);
            GetMasterPivot().rotation = GetRootBone().rotation * weaponBoneMS.rotation;
            
            // Apply pivot point offset
            GetMasterIK().Move(GetPivotPoint().localPosition, 1f);
            GetMasterIK().Rotate(GetPivotPoint().localRotation, 1f);
            
            LocRot cachedBaseHip = new LocRot(GetMasterPivot());
            
            // Apply absolute aiming to the base pose
            ApplyAbsAim(GetRootBone().TransformPoint(aimTargetMS));

            // Calculate the delta between Base Aim and Base Hip poses
            Vector3 deltaT = GetMasterPivot().position - cachedBaseHip.position;
            Quaternion deltaR = Quaternion.Inverse(cachedBaseHip.rotation) * GetMasterPivot().rotation;

            // Finally align sights
            GetMasterPivot().position = cachedIK.position + deltaT;
            GetMasterPivot().rotation = cachedIK.rotation * deltaR;
        }

        // Absolute aiming overrides base animation
        protected virtual void ApplyAbsAim()
        {
            GetMasterPivot().position = aimTarget.position;
            GetMasterPivot().rotation = GetRootBone().rotation;
            
            // Apply scope-based offset
            GetMasterIK().Rotate(interpAimPoint.rotation, 1f);
            GetMasterIK().Move(interpAimPoint.position, 1f);
        }
        
        // Absolute aiming overrides base animation
        protected virtual void ApplyAbsAim(Vector3 target)
        {
            GetMasterPivot().position = target;
            GetMasterPivot().rotation = GetRootBone().rotation;
            
            // Apply scope-based offset
            GetMasterIK().Rotate(interpAimPoint.rotation, 1f);
            GetMasterIK().Move(interpAimPoint.position, 1f);
        }
    }
}