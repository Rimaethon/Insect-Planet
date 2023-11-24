// Designed by KINEMATION, 2023

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    [Serializable]
    public struct AimOffsetBone
    {
        [Bone]
        public Transform bone;
        public Vector2 maxAngle;

        public AimOffsetBone(Transform bone, Vector2 maxAngle)
        {
            this.bone = bone;
            this.maxAngle = maxAngle;
        }
    }

    // Collection of AimOffsetBones, used to rotate spine bones to look around
    [Serializable]
    public struct AimOffset
    {
        [Fold(false)] public List<AimOffsetBone> bones;
        public int indexOffset;

        [HideInInspector] public List<Vector2> angles;
        
        public void Init()
        {
            if (angles == null)
            {
                angles = new List<Vector2>();
            }
            else
            {
                angles.Clear();
            }

            bones ??= new List<AimOffsetBone>();

            for (int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                angles.Add(bone.maxAngle);
            }
        }
        
        public bool IsValid()
        {
            return bones != null && angles != null;
        }

        public bool IsChanged()
        {
            return bones.Count != angles.Count;
        }
    }
    
    // Used for detecting zero-frames
    [Serializable]
    public struct CachedBones
    {
        public (Vector3, Quaternion) pelvis;
        public List<Quaternion> lookUp;
    }

    public class LookLayer : AnimLayer
    {
        [SerializeField, Range(0f, 1f)] protected float handsLayerAlpha;
        [SerializeField] protected float handsLerpSpeed;

        [SerializeField, Range(0f, 1f)] protected float pelvisLayerAlpha = 1f;
        [SerializeField] protected float pelvisLerpSpeed;
        protected float interpPelvis;

        [Header("Offsets")] 
        [SerializeField] protected Vector3 pelvisOffset;
        
        [SerializeField] protected AimOffsetTable aimOffsetTable;
        [SerializeField] protected AimOffset lookUpOffset;
        [SerializeField] protected AimOffset lookRightOffset;
        
        protected Transform[] characterBones;
        [SerializeField] protected AimOffset targetUpOffset;
        [SerializeField] protected AimOffset targetRightOffset;

        [FormerlySerializedAs("enableAutoDistribution")] 
        [SerializeField] protected bool autoDistribution;

        [SerializeField, Range(-90f, 90f)] protected float aimUp;
        [SerializeField, Range(-90f, 90f)] protected float aimRight;

        // Aim rotation lerp speed. If 0, no lag will be applied.
        [SerializeField] protected float smoothAim;

        [Header("Leaning")]
        [SerializeField] [Range(-1, 1)] protected int leanDirection;
        [SerializeField] protected float leanAmount = 45f;
        [SerializeField] protected float leanSpeed;

        [Header("Misc")]
        [SerializeField] protected bool detectZeroFrames = true;
        [SerializeField] protected bool checkZeroFootIK = true;
        [SerializeField] protected bool useRightOffset = true;
        
        protected float leanInput;
        protected float interpHands;
        protected Vector2 lerpedAim;

        protected bool _isEditor = false;

        // Used to detect zero key-frames
        [SerializeField] [HideInInspector] private CachedBones cachedBones;
        [SerializeField] [HideInInspector] private CachedBones cacheRef;

        public void SetAimOffsetTable(AimOffsetTable table)
        {
            if (table == null)
            {
                return;
            }
            
            cachedBones.lookUp.Clear();
            cacheRef.lookUp.Clear();

            float aimOffsetAlpha = core.animGraph.GetPoseProgress();

            // Cache current max angle
            for (int i = 0; i < targetUpOffset.bones.Count; i++)
            {
                var bone = lookUpOffset.bones[i];
                bone.maxAngle = Vector2.Lerp(bone.maxAngle, targetUpOffset.bones[i].maxAngle, aimOffsetAlpha);
                lookUpOffset.bones[i] = bone;
                cachedBones.lookUp.Add(Quaternion.identity);
                cacheRef.lookUp.Add(Quaternion.identity);
            }
            
            for (int i = 0; i < targetRightOffset.bones.Count; i++)
            {
                var bone = lookRightOffset.bones[i];
                bone.maxAngle = Vector2.Lerp(bone.maxAngle, targetRightOffset.bones[i].maxAngle, aimOffsetAlpha);
                lookRightOffset.bones[i] = bone;
            }
            
            aimOffsetTable = table;
            
            // Aim Offset Table contains bone indexes
            for (int i = 0; i < aimOffsetTable.aimOffsetUp.Count; i++)
            {
                var bone = aimOffsetTable.aimOffsetUp[i];
                var newBone = new AimOffsetBone(characterBones[bone.boneIndex], bone.angle);
                targetUpOffset.bones[i] = newBone;
            }
            
            for (int i = 0; i < aimOffsetTable.aimOffsetRight.Count; i++)
            {
                var bone = aimOffsetTable.aimOffsetRight[i];
                var newBone = new AimOffsetBone(characterBones[bone.boneIndex], bone.angle);
                targetRightOffset.bones[i] = newBone;
            }
        }

        public void SetPelvisWeight(float weight)
        {
            pelvisLayerAlpha = Mathf.Clamp01(weight);
        }

        public void SetHandsWeight(float weight)
        {
            handsLayerAlpha = Mathf.Clamp01(weight);
        }

        public override void OnAnimStart()
        {
            _isEditor = !Application.isPlaying;
            elbowsWeight = 1f;
            
            Transform[] bones = null;
            var boneContainer = GetComponentInChildren<BoneContainer>();
            if (boneContainer != null)
            {
                bones = boneContainer.boneContainer.ToArray();
            }
            else
            {
                var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (meshRenderer != null)
                {
                    bones = meshRenderer.bones;
                }
            }
            
            if (bones == null)
            {
                Debug.LogWarning("[LookLayer]: No Skinned Mesh Renderer or Bone Container!");
                return;
            }
            
            characterBones = bones;

            targetUpOffset.bones = new List<AimOffsetBone>();
            targetRightOffset.bones = new List<AimOffsetBone>();
            
            cachedBones.lookUp = new List<Quaternion>();
            cacheRef.lookUp = new List<Quaternion>();

            // If no data asset selected copy the editor data
            if (aimOffsetTable == null)
            {
                foreach (var bone in lookUpOffset.bones)
                {
                    targetUpOffset.bones.Add(bone);
                    cachedBones.lookUp.Add(Quaternion.identity);
                    cacheRef.lookUp.Add(Quaternion.identity);
                }
                
                foreach (var bone in lookRightOffset.bones)
                {
                    targetRightOffset.bones.Add(bone);
                }
                return;
            }
            
            lookUpOffset.bones.Clear();
            lookRightOffset.bones.Clear();
            
            lookUpOffset.angles.Clear();
            lookRightOffset.angles.Clear();

            // Aim Offset Table contains bone indexes
            foreach (var bone in aimOffsetTable.aimOffsetUp)
            {
                targetUpOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookUpOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookUpOffset.angles.Add(bone.angle);
                
                cachedBones.lookUp.Add(Quaternion.identity);
                cacheRef.lookUp.Add(Quaternion.identity);
            }
            
            foreach (var bone in aimOffsetTable.aimOffsetRight)
            {
                targetRightOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookRightOffset.bones.Add(new AimOffsetBone(characterBones[bone.boneIndex], bone.angle));
                lookRightOffset.angles.Add(bone.angle);
            }
        }
        
        public override void OnPreAnimUpdate()
        {
            base.OnPreAnimUpdate();
            if (detectZeroFrames)
            {
                CheckZeroFrames();
            }
            
            UpdateSpineBlending();
        }

        public override void OnAnimUpdate()
        {
            if (BlendLayers())
            {
                return;
            }
            
            RotateSpine();
        }

        public override void OnPostIK()
        {
            if (detectZeroFrames)
            {
                CacheBones();
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            lookUpOffset.Init();
            lookRightOffset.Init();
        }
        
        private void DrawDefaultSpine()
        {
            int count = lookUpOffset.bones.Count;
            
            for (int i = 0; i < count - lookUpOffset.indexOffset; i++)
            {
                var pos = lookUpOffset.bones[i].bone.position;

                if (i > 0)
                {
                    var prevBone = lookUpOffset.bones[i - 1].bone.position;

                    CoreToolkitLib.DrawBone(prevBone, pos, 0.01f);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!drawDebugInfo || lookUpOffset.bones == null)
            {
                return;
            }

            var color = Gizmos.color;
            
            Gizmos.color = Color.cyan;
            DrawDefaultSpine();
            Gizmos.color = color;
        }

        private bool BlendLayers()
        {
            return Mathf.Approximately(smoothLayerAlpha, 0f);
        }

        private void UpdateSpineBlending()
        {
            interpPelvis = CoreToolkitLib.Glerp(interpPelvis, pelvisLayerAlpha * smoothLayerAlpha,
                pelvisLerpSpeed);

            Vector3 pelvisFinal = Vector3.Lerp(Vector3.zero, pelvisOffset, interpPelvis);
            CoreToolkitLib.MoveInBoneSpace(GetRootBone(), core.ikRigData.pelvis, pelvisFinal, 1f);
            
            if (!_isEditor)
            {
                aimUp = GetCharData().totalAimInput.y;
                aimRight = GetCharData().totalAimInput.x;

                if (lookRightOffset.bones.Count == 0 || !useRightOffset)
                {
                    aimRight = 0f;
                }

                leanInput = CoreToolkitLib.Glerp(leanInput, leanAmount * GetCharData().leanDirection,
                    leanSpeed);
            }
            else
            {
                leanInput = CoreToolkitLib.Glerp(leanInput, leanAmount * leanDirection, leanSpeed);
            }
            
            interpHands = CoreToolkitLib.GlerpLayer(interpHands, handsLayerAlpha, handsLerpSpeed);
            
            lerpedAim.y = CoreToolkitLib.GlerpLayer(lerpedAim.y, aimUp, smoothAim);
            lerpedAim.x = CoreToolkitLib.GlerpLayer(lerpedAim.x, aimRight, smoothAim);
        }
        
        private void RotateSpine()
        {
            float alpha = smoothLayerAlpha * (1f - GetCurveValue(CurveLib.Curve_MaskLookLayer));
            float aimOffsetAlpha = core.animGraph.GetPoseProgress();

            if (!Mathf.Approximately(leanInput, 0f))
            {
                for (int i = 0; i < lookRightOffset.bones.Count; i++)
                {
                    var targetBone = targetRightOffset.bones[i];
                    var startBone = lookRightOffset.bones[i];
                    if (_isEditor && targetBone.bone == null)
                    {
                        continue;
                    }

                    var angle = Vector2.Lerp(startBone.maxAngle, targetBone.maxAngle, aimOffsetAlpha);
                    CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, targetBone.bone,
                        Quaternion.Euler(0f, 0f, leanInput / (90f / angle.x)), alpha);
                }
            }

            if (!Mathf.Approximately(lerpedAim.x, 0f))
            {
                for (int i = 0; i < lookRightOffset.bones.Count; i++)
                {
                    var targetBone = targetRightOffset.bones[i];
                    var startBone = lookRightOffset.bones[i];
                    if (_isEditor && targetBone.bone == null)
                    {
                        continue;
                    }

                    var angle = Vector2.Lerp(startBone.maxAngle, targetBone.maxAngle, aimOffsetAlpha);
                    // If new bone is in the aim offset, use the smooth alpha to enable it
                    float angleFraction = lerpedAim.x >= 0f ? angle.y : angle.x;
                    CoreToolkitLib.RotateInBoneSpace(GetRootBone().rotation, targetBone.bone,
                        Quaternion.Euler(0f, lerpedAim.x / (90f / angleFraction), 0f), alpha);
                }
            }

            if (Mathf.Approximately(lerpedAim.y, 0f))
            {
                return;
            }

            Vector3 rightHandLoc = GetRightHand().position;
            Quaternion rightHandRot = GetRightHand().rotation;

            Vector3 leftHandLoc = GetLeftHand().position;
            Quaternion leftHandRot = GetLeftHand().rotation;

            Quaternion parentSpace = transform.rotation * Quaternion.Euler(0f, lerpedAim.x, 0f);
            bool useY = lerpedAim.y >= 0f;
            float baseAngle = lerpedAim.y / 90f;
            
            for (int i = 0; i < lookUpOffset.bones.Count; i++)
            {
                var bone = targetUpOffset.bones[i];
                var startBone = lookUpOffset.bones[i];
                if (_isEditor && bone.bone == null)
                {
                    continue;
                }
                
                var angle = Vector2.Lerp(startBone.maxAngle, bone.maxAngle, aimOffsetAlpha);
                float angleFraction = useY ? angle.y : angle.x;
                
                CoreToolkitLib.RotateInBoneSpace(parentSpace, bone.bone,
                    Quaternion.Euler(baseAngle * angleFraction, 0f, 0f), alpha);
            }
            
            GetRightHand().position = Vector3.Lerp(rightHandLoc, GetRightHand().position, interpHands);
            GetRightHand().rotation = Quaternion.Slerp(rightHandRot, GetRightHand().rotation, interpHands);

            GetLeftHand().position = Vector3.Lerp(leftHandLoc, GetLeftHand().position, interpHands);
            GetLeftHand().rotation = Quaternion.Slerp(leftHandRot, GetLeftHand().rotation, interpHands);
        }

        private void OnValidate()
        {
            if (cachedBones.lookUp == null)
            {
                cachedBones.lookUp ??= new List<Quaternion>();
                cacheRef.lookUp ??= new List<Quaternion>();
            }

            if (!lookUpOffset.IsValid() || lookUpOffset.IsChanged())
            {
                lookUpOffset.Init();

                cachedBones.lookUp.Clear();
                cacheRef.lookUp.Clear();

                for (int i = 0; i < lookUpOffset.bones.Count; i++)
                {
                    cachedBones.lookUp.Add(Quaternion.identity);
                    cacheRef.lookUp.Add(Quaternion.identity);
                }
            }

            if (!lookRightOffset.IsValid() || lookRightOffset.IsChanged())
            {
                lookRightOffset.Init();
            }

            void Distribute(ref AimOffset aimOffset)
            {
                if (autoDistribution)
                {
                    bool enable = false;
                    int divider = 1;
                    float sum = 0f;

                    int boneCount = aimOffset.bones.Count - aimOffset.indexOffset;

                    for (int i = 0; i < boneCount; i++)
                    {
                        if (enable)
                        {
                            var bone = aimOffset.bones[i];
                            bone.maxAngle.x = (90f - sum) / divider;
                            aimOffset.bones[i] = bone;
                            continue;
                        }

                        if (!Mathf.Approximately(aimOffset.bones[i].maxAngle.x, aimOffset.angles[i].x))
                        {
                            divider = boneCount - (i + 1);
                            enable = true;
                        }

                        sum += aimOffset.bones[i].maxAngle.x;
                    }

                    enable = false;
                    divider = 1;
                    sum = 0f;

                    for (int i = 0; i < boneCount; i++)
                    {
                        if (enable)
                        {
                            var bone = aimOffset.bones[i];
                            bone.maxAngle.y = (90f - sum) / divider;
                            aimOffset.bones[i] = bone;
                            continue;
                        }

                        if (!Mathf.Approximately(aimOffset.bones[i].maxAngle.y, aimOffset.angles[i].y))
                        {
                            divider = boneCount - (i + 1);
                            enable = true;
                        }

                        sum += aimOffset.bones[i].maxAngle.y;
                    }

                    // Copy max angles to angles list
                    for (int i = 0; i < boneCount; i++)
                    {
                        aimOffset.angles[i] = aimOffset.bones[i].maxAngle;
                    }
                }
            }

            if (lookUpOffset.bones.Count > 0)
            {
                Distribute(ref lookUpOffset);
            }

            if (lookRightOffset.bones.Count > 0)
            {
                Distribute(ref lookRightOffset);
            }
        }

        private void CheckZeroFrames()
        {
            if (cachedBones.pelvis.Item1 == core.ikRigData.pelvis.localPosition)
            {
                core.ikRigData.pelvis.localPosition = cacheRef.pelvis.Item1;
            }

            if (cachedBones.pelvis.Item2 == core.ikRigData.pelvis.localRotation)
            {
                core.ikRigData.pelvis.localRotation = cacheRef.pelvis.Item2;

                if (checkZeroFootIK)
                {
                    core.ikRigData.rightFoot.Retarget();
                    core.ikRigData.leftFoot.Retarget();
                }
            }

            cacheRef.pelvis.Item2 = core.ikRigData.pelvis.localRotation;

            bool bZeroSpine = false;
            for (int i = 0; i < cachedBones.lookUp.Count; i++)
            {
                var bone = lookUpOffset.bones[i].bone;
                if (bone == null || bone == core.ikRigData.pelvis)
                {
                    continue;
                }

                if (cachedBones.lookUp[i] == bone.localRotation)
                {
                    bZeroSpine = true;
                    bone.localRotation = cacheRef.lookUp[i];
                }
            }

            if (bZeroSpine)
            {
                core.ikRigData.masterDynamic.Retarget();
                core.ikRigData.rightHand.Retarget();
                core.ikRigData.leftHand.Retarget();
            }

            cacheRef.pelvis.Item1 = core.ikRigData.pelvis.localPosition;

            for (int i = 0; i < lookUpOffset.bones.Count; i++)
            {
                var bone = lookUpOffset.bones[i].bone;
                if (bone == null)
                {
                    continue;
                }

                cacheRef.lookUp[i] = bone.localRotation;
            }
        }

        private void CacheBones()
        {
            cachedBones.pelvis.Item1 = core.ikRigData.pelvis.localPosition;
            cachedBones.pelvis.Item2 = core.ikRigData.pelvis.localRotation;

            for (int i = 0; i < lookUpOffset.bones.Count; i++)
            {
                var bone = lookUpOffset.bones[i].bone;
                if (bone == null || bone == core.ikRigData.pelvis)
                {
                    continue;
                }

                cachedBones.lookUp[i] = bone.localRotation;
            }
        }
        
#if UNITY_EDITOR
        
        
        public AimOffsetTable SaveTable()
        {
            Transform[] bones = null;
            var boneContainer = GetComponentInChildren<BoneContainer>();
            if (boneContainer != null)
            {
                bones = boneContainer.boneContainer.ToArray();
            }
            else
            {
                var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (meshRenderer != null)
                {
                    bones = meshRenderer.bones;
                }
            }

            if (bones == null)
            {
                Debug.LogWarning("[LookLayer]: No Skinned Mesh Renderer or Bone Container!");
                return null;
            }

            int GetBoneIndex(Transform target)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    if (target == bones[i])
                    {
                        return i;
                    }
                }

                return 0;
            }

            if (aimOffsetTable != null)
            {
                aimOffsetTable.aimOffsetUp.Clear();
                aimOffsetTable.aimOffsetRight.Clear();

                foreach (var aimOffsetBone in lookUpOffset.bones)
                {
                    Vector2 angle = aimOffsetBone.maxAngle;
                    int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                    aimOffsetTable.aimOffsetUp.Add(new BoneAngle(boneIndex, angle));
                }

                foreach (var aimOffsetBone in lookRightOffset.bones)
                {
                    Vector2 angle = aimOffsetBone.maxAngle;
                    int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                    aimOffsetTable.aimOffsetRight.Add(new BoneAngle(boneIndex, angle));
                }

                return aimOffsetTable;
            }

            aimOffsetTable = ScriptableObject.CreateInstance<AimOffsetTable>();
            aimOffsetTable.aimOffsetRight = new List<BoneAngle>();
            aimOffsetTable.aimOffsetUp = new List<BoneAngle>();

            foreach (var aimOffsetBone in lookUpOffset.bones)
            {
                Vector2 angle = aimOffsetBone.maxAngle;
                int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                aimOffsetTable.aimOffsetUp.Add(new BoneAngle(boneIndex, angle));
            }

            foreach (var aimOffsetBone in lookRightOffset.bones)
            {
                Vector2 angle = aimOffsetBone.maxAngle;
                int boneIndex = GetBoneIndex(aimOffsetBone.bone);
                aimOffsetTable.aimOffsetRight.Add(new BoneAngle(boneIndex, angle));
            }

            return aimOffsetTable;
        }
#endif
    }
}