// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    [System.Serializable, CreateAssetMenu(fileName = "NewIKAnimation", menuName = "FPS Animator/IKAnimation")]
    public class IKAnimation : ScriptableObject
    {
        public VectorCurve rot = new VectorCurve(new Keyframe[] {new (0, 0), new (1, 0)});
        public VectorCurve loc = new VectorCurve(new Keyframe[] {new (0, 0), new (1, 0)});
        
        public float blendSpeed;
        public float playRate;
    }
}