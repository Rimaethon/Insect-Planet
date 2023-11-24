// Designed by KINEMATION, 2023

using System.Collections.Generic;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [System.Serializable]
    public class AimOffsetTable : ScriptableObject
    {
        public List<BoneAngle> aimOffsetUp;
        public List<BoneAngle> aimOffsetRight;
    }
}