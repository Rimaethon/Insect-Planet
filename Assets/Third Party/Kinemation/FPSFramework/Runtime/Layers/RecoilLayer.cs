// Designed by KINEMATION, 2023

using System;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class RecoilLayer : AnimLayer
    {
        [SerializeField] private bool useMeshSpace;
        public override void OnAnimUpdate()
        {
            var masterDynamic = GetMasterPivot();
            var recoilAnim = core.characterData.recoilAnim;
            
            LocRot baseT = new LocRot(masterDynamic.position, masterDynamic.rotation);
            
            OffsetMasterPivot(GetRigData().adsPivotOffset);

            if (useMeshSpace)
            {
                CoreToolkitLib.MoveInBoneSpace(GetRootBone(), masterDynamic,
                    recoilAnim.position, 1f);
                CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, masterDynamic,
                    recoilAnim.rotation, 1f);
            }
            else
            {
                CoreToolkitLib.MoveInBoneSpace(masterDynamic, masterDynamic,
                    recoilAnim.position, 1f);
                CoreToolkitLib.RotateInBoneSpace(masterDynamic.rotation, masterDynamic,
                    recoilAnim.rotation, 1f);
            }
            
            OffsetMasterPivot(-GetRigData().adsPivotOffset);
            
            LocRot newT = new LocRot(masterDynamic.position, masterDynamic.rotation);

            masterDynamic.position = Vector3.Lerp(baseT.position, newT.position, smoothLayerAlpha);
            masterDynamic.rotation = Quaternion.Slerp(baseT.rotation, newT.rotation, smoothLayerAlpha);
        }
    }
}
