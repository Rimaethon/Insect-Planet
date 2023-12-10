// Designed by KINEMATION, 2023

using System;
using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;
using UnityEngine.SearchService;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    [Serializable]
    public struct LookAtBone
    {
        [Bone] public Transform bone;
        public Vector3 axis;
        [Range(0f, 1f)] public float weight;
    }
    
    public class LookAtLayer : AnimLayer
    {
        [SerializeField] private List<LookAtBone> lookAtBones;
        [SerializeField] private Transform lookSource;
        [SerializeField] private Transform lookAtTarget;
        [SerializeField] private float maxDistance = 0f;
        [SerializeField] private Vector3 lookSourceAxis = Vector3.forward;
        
        private void DrawLookAtSpine()
        {
            int count = lookAtBones.Count;
            
            for (int i = 0; i < count; i++)
            {
                if (lookAtBones[i].bone == null)
                {
                    continue;
                }
                
                var pos = lookAtBones[i].bone.position;

                if (i > 0)
                {
                    var prevBone = lookAtBones[i - 1].bone.position;
                    CoreToolkitLib.DrawBone(prevBone, pos, 0.01f);
                }
            }

            if (GetMasterPivot() != null)
            {
                var start = lookSource.position;
                var end = lookSource.position + lookSource.TransformDirection(lookSourceAxis) * 5f;
                Gizmos.DrawLine(start, end);
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!drawDebugInfo || lookAtBones == null)
            {
                return;
            }

            var color = Gizmos.color;
            
            Gizmos.color = Color.cyan;
            DrawLookAtSpine();
            Gizmos.color = color;
        }
        
        private void LookAt()
        {
            if (lookAtTarget == null || lookSource == null || Mathf.Approximately(smoothLayerAlpha, 0f))
            {
                return;
            }
            
            foreach (var bone in lookAtBones)
            {
                if (bone.bone == null || Mathf.Approximately(bone.weight, 0f))
                {
                    continue;
                }
                
                Quaternion transformRot = bone.bone.rotation;
        
                Vector3 worldAxis = bone.bone.TransformDirection(bone.axis);
                Vector3 delta = (lookAtTarget.position - bone.bone.position).normalized;
        
                Quaternion lookRot = Quaternion.FromToRotation(worldAxis, delta);

                Quaternion finalRot = lookRot * transformRot;
                bone.bone.rotation = finalRot;
                
                Vector3 sourceToTargetDir = (lookAtTarget.position - lookSource.position).normalized;
                Vector3 worldSourceAxis = lookSource.TransformDirection(lookSourceAxis);
                Quaternion desiredLookRot = Quaternion.FromToRotation(worldSourceAxis, sourceToTargetDir);

                finalRot = Quaternion.Slerp(transformRot, desiredLookRot * bone.bone.rotation,
                    bone.weight * smoothLayerAlpha);
                bone.bone.rotation = finalRot;
            }
        }
        
        public void ToggleLookAt(bool enable = false, Transform target = null)
        {
            SetLayerAlpha(enable ? 1f : 0f);

            if (target != null)
            {
                lookAtTarget = target;
            }
        }

        public void SetLookSource(Transform source)
        {
            if (source != null)
            {
                lookSource = source;
            }
        }

        public override void OnPreAnimUpdate()
        {
            bool bValid = lookSource != null && lookAtTarget != null
                                             && (lookAtTarget.position - lookSource.position).magnitude <= maxDistance;
            float alpha = layerAlpha * (bValid ? 1f : 0f);
            smoothLayerAlpha = CoreToolkitLib.GlerpLayer(smoothLayerAlpha, alpha, lerpSpeed);
            if (!string.IsNullOrEmpty(curveName))
            {
                smoothLayerAlpha *= GetAnimator().GetFloat(curveName);
            }
        }

        public override void OnAnimUpdate()
        {
            if (Mathf.Approximately(smoothLayerAlpha, 0f))
            {
                return;
            }
                
            LookAt();
        }
    }
}
