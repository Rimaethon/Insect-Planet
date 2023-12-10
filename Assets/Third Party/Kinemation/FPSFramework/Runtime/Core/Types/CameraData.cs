// Designed by KINEMATION, 2023

using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [System.Serializable, CreateAssetMenu(fileName = "NewAimData", menuName = "FPS Animator/CameraData")]
    public class CameraData : ScriptableObject
    {
        public float baseFOV = 90f;
        public float aimFOV = 50f;
        public AnimationCurve fovCurve = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 1f)
        }); 
        
        public float aimSpeed = 1f;
        public float extraSmoothing = 0f;
    }
}