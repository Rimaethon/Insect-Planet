using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    [System.Serializable, CreateAssetMenu(fileName = "NewIKPose", menuName = "FPS Animator/IKPose")]
    public class IKPose : ScriptableObject
    {
        public LocRot pose = LocRot.identity;
        [Min(0f)] public float blendInSpeed = 0f;
        [Min(0f)] public float blendOutSpeed = 0f;
    }
}