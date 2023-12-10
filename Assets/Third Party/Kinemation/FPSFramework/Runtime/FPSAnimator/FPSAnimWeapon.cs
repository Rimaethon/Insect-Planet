// Designed by KINEMATION, 2023

using UnityEngine;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.Recoil;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    public abstract class FPSAnimWeapon : MonoBehaviour
    {
        public WeaponAnimAsset weaponAsset;
        public WeaponTransformData weaponTransformData;
        
        public AimOffsetTable aimOffsetTable;
        public RecoilAnimData recoilData;
    
        public FireMode fireMode = FireMode.Semi;
        public float fireRate = 600f;
        public int burstAmount = 0;
        public AnimSequence overlayPose;
        public LocRot weaponBone = LocRot.identity;

        // Returns the aim point by default
        public virtual Transform GetAimPoint()
        {
            return weaponTransformData.aimPoint;
        }

#if UNITY_EDITOR
        public void SetupWeapon()
        {
            Transform FindPoint(Transform target, string searchName)
            {
                foreach (Transform child in target)
                {
                    if (child.name.ToLower().Contains(searchName.ToLower()))
                    {
                        return child;
                    }
                }

                return null;
            }
            
            if (weaponTransformData.pivotPoint == null)
            {
                var found = FindPoint(transform, "pivot");
                weaponTransformData.pivotPoint = found == null ? new GameObject("PivotPoint").transform : found;
                weaponTransformData.pivotPoint.parent = transform;
            }
            
            if (weaponTransformData.aimPoint == null)
            {
                var found = FindPoint(transform, "aimpoint");
                weaponTransformData.aimPoint = found == null ? new GameObject("AimPoint").transform : found;
                weaponTransformData.aimPoint.parent = transform;
            }
        }

        public void SavePose()
        {
            weaponBone.position = transform.localPosition;
            weaponBone.rotation = transform.localRotation;
        }
#endif
    }
}