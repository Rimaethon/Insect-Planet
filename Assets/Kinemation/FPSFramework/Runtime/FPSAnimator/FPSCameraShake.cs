// Designed by KINEMATION, 2023

using UnityEngine;

using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.Core.Types;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    [CreateAssetMenu(fileName = "NewCameraShake", menuName = "FPS Animator/FPSCameraShake")]
    public class FPSCameraShake : ScriptableObject
    {
        [Fold(false)] public CameraShakeInfo shakeInfo
            = new CameraShakeInfo(new[] {new Keyframe(0f, 0f), new Keyframe(1f, 0f)},
                1f, 1f);
    }
}