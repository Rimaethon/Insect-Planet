// Designed by KINEMATION, 2023

using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    [System.Serializable]
    public class AnimSequence : ScriptableObject
    {
        [Tooltip("Select your animation")]
        public AnimationClip clip = null;
        [Tooltip("Applied to the spine root bone")]
        public Quaternion spineRotation = Quaternion.identity;
        [Tooltip("What bones will be animated")]
        public AvatarMask mask = null;
        [Tooltip("Smooth blend in/out parameters")]
        public BlendTime blendTime;
        public List<AnimCurve> curves;

        public float GetTimeAtFrame(int frame)
        {
            if (clip == null)
            {
                return 0f;
            }

            frame = frame < 0 ? frame * -1 : frame;
            return frame / (clip.frameRate * clip.length);
        }

        public float GetNormTimeAtFrame(int frame)
        {
            if (clip == null)
            {
                return 0f;
            }
            
            return GetTimeAtFrame(frame) / clip.length;
        }
    }
}