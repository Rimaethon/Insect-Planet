// Designed by KINEMATION, 2023

using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Recoil
{
    [System.Serializable, CreateAssetMenu(fileName = "NewRecoilAnimData", menuName = "FPS Animator/RecoilAnimData")]
    public class RecoilAnimData : ScriptableObject
    {
        [Header("Rotation Targets")]
        public Vector2 pitch;
        public Vector4 roll = new Vector4(0f, 0f, 0f, 0f);
        public Vector4 yaw = new Vector4(0f, 0f, 0f, 0f);

        [Header("Translation Targets")] 
        public Vector2 kickback;
        public Vector2 kickUp;
        public Vector2 kickRight;
    
        [Header("Aiming Multipliers")]
        public Vector3 aimRot;
        public Vector3 aimLoc;
    
        [Header("Auto/Burst Settings")]
        public Vector3 smoothRot;
        public Vector3 smoothLoc;
        
        public Vector3 extraRot;
        public Vector3 extraLoc;
    
        [Header("Noise Layer")]
        public Vector2 noiseX;
        public Vector2 noiseY;

        public Vector2 noiseAccel;
        public Vector2 noiseDamp;
    
        public float noiseScalar = 1f;
    
        [Header("Pushback Layer")]
        public float pushAmount = 0f;
        public float pushAccel;
        public float pushDamp;

        [Header("Misc")]
        public bool smoothRoll;
        public float playRate;
    
        [Header("Recoil Curves")]
        public RecoilCurves recoilCurves = new RecoilCurves(
            new[] { new Keyframe(0f, 0f), new Keyframe(1f, 0f) });
    }
}
