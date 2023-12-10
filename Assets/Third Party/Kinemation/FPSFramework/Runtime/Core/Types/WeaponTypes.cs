// Designed by KINEMATION, 2023

using System;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [Serializable]
    public struct FreeAimData
    {
        public float scalar;
        public float maxValue;
        public float speed;
    }

    [Serializable]
    public struct MoveSwayData
    {
        public Vector3 maxMoveLocSway;
        public Vector3 maxMoveRotSway;

        public VectorSpringData moveLocSway;
        public VectorSpringData moveRotSway;

        public Vector3 locSpeed;
        public Vector3 rotSpeed;
    }
    
    [Serializable]
    public struct GunBlockData
    {
        public float weaponLength;
        public float startOffset;
        public float threshold;
        public LocRot restPose;

        public GunBlockData(LocRot pose)
        {
            restPose = pose;
            weaponLength = startOffset = threshold = 0f;
        }
    }
    
    [Serializable]
    public struct AdsData
    {
        public CameraData cameraData;
        public AdsBlend adsTranslationBlend;
        public AdsBlend adsRotationBlend;
        public LocRot pointAimOffset;
        public float aimSpeed;
        public float changeSightSpeed;
        public float pointAimSpeed;

        public AdsData(float speed)
        {
            cameraData = null;
            pointAimOffset = LocRot.identity;
            aimSpeed = changeSightSpeed = pointAimSpeed = speed;
            adsTranslationBlend = adsRotationBlend = new AdsBlend();
        }
    }
    
    [Serializable]
    public struct WeaponTransformData
    {
        public Transform pivotPoint;
        public Transform aimPoint;
        public Transform leftHandTarget;
    }
}