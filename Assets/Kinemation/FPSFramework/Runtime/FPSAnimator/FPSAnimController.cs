// Designed by KINEMATION, 2023

using UnityEngine;
using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;
using UnityEngine.Events;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    // Animation Controller Interface
    // Make sure to derive your controller from this class
    public abstract class FPSAnimController : MonoBehaviour
    {
        private CoreAnimComponent fpsAnimator;
        private FPSCamera fpsCamera;
        private LookLayer internalLookLayer;
        protected RecoilAnimation recoilComponent;
        protected CharAnimData charAnimData;
        
        // Used primarily for function calls from Animation Events
        // Runs once at the beginning of the next update
        protected UnityEvent queuedAnimEvents;
        
        protected CoreAnimGraph GetAnimGraph()
        {
            return fpsAnimator.animGraph;
        }

        // Call this once when the character is initialized
        protected void InitAnimController()
        {
            fpsAnimator = GetComponentInChildren<CoreAnimComponent>();
            fpsAnimator.animGraph.InitPlayableGraph();
            fpsAnimator.InitializeLayers();

            recoilComponent = GetComponentInChildren<RecoilAnimation>();
            charAnimData = new CharAnimData();

            fpsCamera = GetComponentInChildren<FPSCamera>();
            internalLookLayer = GetComponentInChildren<LookLayer>();

            if (fpsCamera != null)
            {
                fpsCamera.rootBone = fpsAnimator.ikRigData.rootBone;
            }
        }

        // Call this once when the character is initialized
        protected void InitAnimController(UnityAction cameraDelegate)
        {
            InitAnimController();
            fpsAnimator.onPostUpdate.AddListener(cameraDelegate);

            if (fpsCamera == null) return;
            fpsAnimator.onPostUpdate.AddListener(fpsCamera.UpdateCamera);
        }
        
        // Call this when equipping a new weapon
        protected void InitWeapon(FPSAnimWeapon weapon)
        {
            recoilComponent.Init(weapon.recoilData, weapon.fireRate, weapon.fireMode);
            fpsAnimator.OnGunEquipped(weapon.weaponAsset, weapon.weaponTransformData);
            
            fpsAnimator.ikRigData.weaponTransform = weapon.weaponBone;
            internalLookLayer.SetAimOffsetTable(weapon.aimOffsetTable);

            fpsAnimator.OnPrePoseSampled();
            PlayPose(weapon.overlayPose);
            fpsAnimator.OnPoseSampled();

            if (fpsCamera != null)
            {
                fpsCamera.cameraData = weapon.weaponAsset.adsData.cameraData;
            }
        }

        // Call this when changing sights
        protected void InitAimPoint(FPSAnimWeapon weapon)
        {
            fpsAnimator.OnSightChanged(weapon.GetAimPoint());
        }

        // Call this during Update after all the gameplay logic
        protected void UpdateAnimController()
        {
            if (queuedAnimEvents != null)
            {
                queuedAnimEvents.Invoke();
                queuedAnimEvents = null;
            }
            
            charAnimData.recoilAnim = new LocRot(recoilComponent.OutLoc, Quaternion.Euler(recoilComponent.OutRot));
            fpsAnimator.SetCharData(charAnimData);
            
            fpsAnimator.animGraph.UpdateGraph();
            fpsAnimator.UpdateCoreComponent();

            if (fpsCamera != null)
            {
                float Pitch = GetAnimGraph().GetCurveValue(CurveLib.Curve_Camera_Pitch);
                float Yaw = GetAnimGraph().GetCurveValue(CurveLib.Curve_Camera_Yaw);
                float Roll = GetAnimGraph().GetCurveValue(CurveLib.Curve_Camera_Roll);
                
                Quaternion rot = Quaternion.Euler(Pitch, Yaw, Roll);
                fpsCamera.UpdateCameraAnimation(rot.normalized);
            }
        }

        protected void OnInputAim(bool isAiming)
        {
            if (fpsCamera != null)
            {
                fpsCamera.isAiming = isAiming;
            }
        }

        // Call this to play a Camera shake
        protected void PlayCameraShake(FPSCameraShake shake)
        {
            if (fpsCamera != null)
            {
                fpsCamera.PlayShake(shake.shakeInfo);
            }
        }
        
        protected void PlayController(RuntimeAnimatorController controller, AnimSequence motion)
        {
            if (motion == null) return;
            fpsAnimator.animGraph.PlayController(controller, motion.clip, motion.blendTime.blendInTime);
        }
        
        // Call this to play a static pose on the character upper body
        protected void PlayPose(AnimSequence motion)
        {
            if (motion == null) return;
            fpsAnimator.animGraph.PlayPose(motion.clip, motion.blendTime.blendInTime);
        }

        // Call this to play an animation on the character upper body
        protected void PlayAnimation(AnimSequence motion, float startTime = 0f)
        {
            if (motion == null) return;

            var animTime = new AnimTime()
            {
                blendTime = motion.blendTime,
                startTime = startTime
            };
            
            fpsAnimator.animGraph.PlayAnimation(motion.clip, animTime, motion.spineRotation, motion.curves.ToArray());
        }
        
        protected void StopAnimation(float blendTime = 0f)
        {
            fpsAnimator.animGraph.StopAnimation(blendTime);
        }
    }
}