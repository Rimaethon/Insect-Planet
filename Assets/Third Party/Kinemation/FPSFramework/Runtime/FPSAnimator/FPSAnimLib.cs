// Designed by KINEMATION, 2023

using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    public static class FPSAnimLib
    {
        public static float ExpDecay(float a, float b, float speed, float deltaTime)
        {
            return Mathf.Lerp(a, b, 1 - Mathf.Exp(-speed * deltaTime));
        }
        
        public static Quaternion ExpDecay(Quaternion a, Quaternion b, float speed, float deltaTime)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-speed * deltaTime));
        }

        public static Vector2 ExpDecay(Vector2 a, Vector2 b, float speed, float deltaTime)
        {
            return Vector2.Lerp(a, b, 1 - Mathf.Exp(-speed * deltaTime));
        }

        public static Quaternion RotateInSpace(Quaternion space, Quaternion original, Quaternion offset, float alpha)
        {
            Quaternion outRot = offset * (Quaternion.Inverse(space) * original);
            return Quaternion.Slerp(original, space * outRot, alpha);
        }
        public static float ExpDecayAlpha(float speed, float deltaTime)
        {
            return 1 - Mathf.Exp(-speed * deltaTime);
        }
    }
}