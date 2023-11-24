// Designed by KINEMATION, 2023

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine.Events;

namespace Kinemation.FPSFramework.Runtime.Core.Components
{
    // DynamicBone is essentially an IK bone
    [Serializable]
    public struct DynamicBone
    {
        [Tooltip("Target Skeleton Bone")] 
        [Bone] public Transform target;

        [Tooltip("Elbow/Knee Skeleton Bone")] 
        [Bone] public Transform hintTarget;
        
        [Tooltip("Elbow/Knee IK Object")]
        public GameObject hintObj;

        [Tooltip("Target IK Object")]
        public GameObject obj;

        public void Retarget()
        {
            if (target == null)
            {
                return;
            }

            obj.transform.position = target.position;
            obj.transform.rotation = target.rotation;

            if (hintObj == null || hintTarget == null)
            {
                return;
            }
            
            hintObj.transform.position = hintTarget.position;
            hintObj.transform.rotation = hintTarget.rotation;
        }

        public void Rotate(Quaternion parent, Quaternion rotation, float alpha)
        {
            CoreToolkitLib.RotateInBoneSpace(parent, obj.transform, rotation, alpha);
        }

        public void Rotate(Quaternion rotation, float alpha)
        {
            CoreToolkitLib.RotateInBoneSpace(obj.transform.rotation, obj.transform, rotation, alpha);
        }

        public void Move(Transform parent, Vector3 offset, float alpha)
        {
            CoreToolkitLib.MoveInBoneSpace(parent, obj.transform, offset, alpha);
        }

        public void Move(Vector3 offset, float alpha)
        {
            CoreToolkitLib.MoveInBoneSpace(obj.transform, obj.transform, offset, alpha);
        }
    }

    // Essential skeleton data, used by Anim Layers
    [Serializable]
    public struct DynamicRigData
    {
        public AnimationClip tPose;
    
        public Animator animator;
        [Bone] public Transform pelvis;

        [Tooltip("Check if your rig has an IK gun bone")]
        public Transform weaponBone;
        
        [Tooltip("Check if your rig has an IK gun bone")]
        public Transform weaponBoneAdditive;

        public Transform weaponBoneRight;
        public Transform weaponBoneLeft;

        [HideInInspector] public float weaponBoneWeight;
        [HideInInspector] public LocRot weaponTransform;
        [HideInInspector] public Vector3 adsPivotOffset;

        public DynamicBone masterDynamic;
        public DynamicBone rightHand;
        public DynamicBone leftHand;
        public DynamicBone rightFoot;
        public DynamicBone leftFoot;

        [Tooltip("Used for mesh space calculations")] [Bone]
        public Transform spineRoot;

        [Bone] public Transform rootBone;

        public Quaternion GetPelvisMS()
        {
            return Quaternion.Inverse(rootBone.rotation) * pelvis.rotation;
        }

        public void RetargetHandBones()
        {
            weaponBoneRight.position = weaponBone.position;
            weaponBoneRight.rotation = weaponBone.rotation;

            weaponBoneLeft.position = weaponBone.position;
            weaponBoneLeft.rotation = weaponBone.rotation;
        }

        public void RetargetWeaponBone()
        {
            weaponBone.position = rootBone.TransformPoint(weaponTransform.position);
            weaponBone.rotation = rootBone.rotation * weaponTransform.rotation;
        }

        public void UpdateWeaponParent()
        {
            LocRot boneDefault = new LocRot(weaponBoneRight);
            LocRot boneRight = new LocRot(masterDynamic.obj.transform);
            LocRot boneLeft = new LocRot(weaponBoneLeft);

            LocRot result = LocRot.identity;
            if (weaponBoneWeight >= 0f)
            {
                result = CoreToolkitLib.Lerp(boneDefault, boneRight, weaponBoneWeight);
            }
            else
            {
                result = CoreToolkitLib.Lerp(boneDefault, boneLeft, -weaponBoneWeight);
            }

            masterDynamic.obj.transform.position = result.position;
            masterDynamic.obj.transform.rotation = result.rotation;
        }

        public void AlignWeaponBone(Vector3 offset)
        {
            if (!Application.isPlaying) return;

            masterDynamic.Move(offset, 1f);

            weaponBone.position = masterDynamic.obj.transform.position;
            weaponBone.rotation = masterDynamic.obj.transform.rotation;
        }

        public void Retarget()
        {
            rightFoot.Retarget();
            leftFoot.Retarget();
        }
    }

    [Serializable]
    public abstract class AnimLayer : MonoBehaviour
    {
        [Header("Layer Blending")] 
        [SerializeField, AnimCurveName(true)] protected string curveName;

        [SerializeField, Range(0f, 1f)] protected float layerAlpha = 1f;
        [SerializeField] protected float lerpSpeed;
        [Range(0f, 1f)] public float elbowsWeight = 0f;
        protected float smoothLayerAlpha;
        
        [Header("Misc")] 
        [SerializeField] protected bool drawDebugInfo = true;
        [SerializeField] public bool runInEditor;
        protected CoreAnimComponent core;

        public void SetLayerAlpha(float weight)
        {
            layerAlpha = Mathf.Clamp01(weight);
        }

        public float GetLayerAlpha()
        {
            return smoothLayerAlpha;
        }

        // Called in Start()
        public virtual void OnAnimStart()
        {
        }
        
        // Called before the main anim update cycle
        public virtual void OnPreAnimUpdate()
        {
            float target = !string.IsNullOrEmpty(curveName) ? GetAnimator().GetFloat(curveName) : layerAlpha;
            smoothLayerAlpha = CoreToolkitLib.GlerpLayer(smoothLayerAlpha, Mathf.Clamp01(target), lerpSpeed);
        }
        
        // Main anim update
        public virtual void OnAnimUpdate()
        {
        }
        
        // Called after the IK is applied
        public virtual void OnPostIK()
        {
        }

        // Called when an overlay pose is sampled
        public virtual void OnPoseSampled()
        {
        }

        protected virtual void Awake()
        {
        }

        public void OnEnable()
        {
            core = GetComponent<CoreAnimComponent>();
        }
        
        protected WeaponAnimAsset GetGunAsset()
        {
            return core.weaponAsset;
        }

        protected CharAnimData GetCharData()
        {
            return core.characterData;
        }

        protected Transform GetMasterPivot()
        {
            return core.ikRigData.masterDynamic.obj.transform;
        }

        protected Transform GetRootBone()
        {
            return core.ikRigData.rootBone;
        }

        protected Transform GetPelvis()
        {
            return core.ikRigData.pelvis;
        }

        protected DynamicBone GetMasterIK()
        {
            return core.ikRigData.masterDynamic;
        }

        protected DynamicBone GetRightHandIK()
        {
            return core.ikRigData.rightHand;
        }

        protected DynamicBone GetLeftHandIK()
        {
            return core.ikRigData.leftHand;
        }
        
        protected Transform GetRightHand()
        {
            return core.ikRigData.rightHand.obj.transform;
        }

        protected Transform GetLeftHand()
        {
            return core.ikRigData.leftHand.obj.transform;
        }

        protected Transform GetRightFoot()
        {
            return core.ikRigData.rightFoot.obj.transform;
        }

        protected Transform GetLeftFoot()
        {
            return core.ikRigData.leftFoot.obj.transform;
        }

        protected Animator GetAnimator()
        {
            return core.ikRigData.animator;
        }

        protected DynamicRigData GetRigData()
        {
            return core.ikRigData;
        }

        protected float GetCurveValue(string curve)
        {
            return core.animGraph.GetCurveValue(curve);
        }

        // Offsets master pivot only, without affecting the child IK bones
        // Useful if weapon has multiple pivots
        protected void OffsetMasterPivot(LocRot offset, float alpha = 1f)
        {
            LocRot rightHandTip = new LocRot(GetRightHand());
            LocRot leftHandTip = new LocRot(GetLeftHand());

            LocRot rightElbow = new LocRot(GetRightHandIK().hintTarget);
            LocRot leftElbow = new LocRot(GetLeftHandIK().hintTarget);

            GetMasterIK().Move(GetMasterPivot(), offset.position, alpha);
            GetMasterIK().Rotate(GetMasterPivot().rotation, offset.rotation, alpha);

            GetRightHand().position = rightHandTip.position;
            GetRightHand().rotation = rightHandTip.rotation;

            GetLeftHand().position = leftHandTip.position;
            GetLeftHand().rotation = leftHandTip.rotation;

            GetRightHandIK().hintTarget.position = rightElbow.position;
            GetRightHandIK().hintTarget.rotation = rightElbow.rotation;
            
            GetLeftHandIK().hintTarget.position = leftElbow.position;
            GetLeftHandIK().hintTarget.rotation = leftElbow.rotation;
        }
        
        protected void OffsetMasterPivot(Vector3 offset, float alpha = 1f)
        {
            LocRot rightHandTip = new LocRot(GetRightHand());
            LocRot leftHandTip = new LocRot(GetLeftHand());

            LocRot rightElbow = new LocRot(GetRightHandIK().hintTarget);
            LocRot leftElbow = new LocRot(GetLeftHandIK().hintTarget);

            GetMasterIK().Move(GetMasterPivot(), offset, alpha);

            GetRightHand().position = rightHandTip.position;
            GetRightHand().rotation = rightHandTip.rotation;

            GetLeftHand().position = leftHandTip.position;
            GetLeftHand().rotation = leftHandTip.rotation;

            GetRightHandIK().hintTarget.position = rightElbow.position;
            GetRightHandIK().hintTarget.rotation = rightElbow.rotation;
            
            GetLeftHandIK().hintTarget.position = leftElbow.position;
            GetLeftHandIK().hintTarget.rotation = leftElbow.rotation;
        }

        protected Transform GetPivotPoint()
        {
            return core.weaponTransformData.pivotPoint;
        }
        
        protected Transform GetAimPoint()
        {
            return core.weaponTransformData.aimPoint;
        }

        protected WeaponTransformData GetTransforms()
        {
            return core.weaponTransformData;
        }

        protected Vector3 GetPivotOffset()
        {
            return GetPivotPoint() != null ? GetPivotPoint().localPosition : Vector3.zero;
        }
    }

    [ExecuteInEditMode, AddComponentMenu("FPS Animator")]
    public class CoreAnimComponent : MonoBehaviour
    {
        public UnityEvent onPreUpdate;
        public UnityEvent onPostUpdate;
        
        [FormerlySerializedAs("rigData")] public DynamicRigData ikRigData;
        public CharAnimData characterData;

        public WeaponAnimAsset weaponAsset;
        [HideInInspector] public WeaponTransformData weaponTransformData;

        [HideInInspector] public CoreAnimGraph animGraph;
        [SerializeField] [HideInInspector] private List<AnimLayer> animLayers;
        [SerializeField] private bool useIK = true;

        [SerializeField] private bool drawDebug;

        private bool _updateInEditor = false;
        private float _interpHands;
        private float _interpLayer;

        // General IK weight for hands
        [SerializeField, Range(0f, 1f)] private float handIkWeight = 1f;

        // Global IK weight for feet
        [SerializeField, Range(0f, 1f)] private float legIkWeight = 1f;
        
        private Quaternion pelvisPoseMS;
        
        // Static weapon bone pose in mesh space
        private LocRot weaponBonePose;
        private LocRot weaponBoneSpinePose;
        
        private bool isPivotValid = false;

        private Tuple<float, float> rightHandWeight = new(1f, 1f);
        private Tuple<float, float> leftHandWeight = new(1f, 1f);
        private Tuple<float, float> rightFootWeight = new(1f, 1f);
        private Tuple<float, float> leftFootWeight = new(1f, 1f);

        private void ApplyIK()
        {
            if (!useIK)
            {
                return;
            }

            void SolveIK(DynamicBone tipBone, Tuple<float, float> weights, float sliderWeight)
            {
                if (Mathf.Approximately(sliderWeight, 0f))
                {
                    return;
                }

                float tWeight = sliderWeight * weights.Item1;
                float hWeight = sliderWeight * weights.Item2;

                Transform hintTarget = tipBone.hintObj == null ? tipBone.hintTarget : tipBone.hintObj.transform;

                var lowerBone = tipBone.target.parent;
                CoreToolkitLib.SolveTwoBoneIK(lowerBone.parent, lowerBone, tipBone.target,
                    tipBone.obj.transform, hintTarget, tWeight, tWeight, hWeight);
            }

            SolveIK(ikRigData.rightHand, rightHandWeight, handIkWeight);
            SolveIK(ikRigData.leftHand, leftHandWeight, handIkWeight);
            SolveIK(ikRigData.rightFoot, rightFootWeight, legIkWeight);
            SolveIK(ikRigData.leftFoot, leftFootWeight, legIkWeight);
        }

        private void OnEnable()
        {
            animLayers ??= new List<AnimLayer>();
            animGraph = GetComponent<CoreAnimGraph>();

            if (animGraph == null)
            {
                animGraph = gameObject.AddComponent<CoreAnimGraph>();
            }

            foreach (var layer in animLayers)
            {
                layer.OnEnable();
            }
        }

        public void InitializeLayers()
        {
            foreach (var layer in animLayers)
            {
                layer.OnAnimStart();
            }
            
            ikRigData.weaponTransform = LocRot.identity;
            ikRigData.weaponBoneRight.localPosition = ikRigData.weaponBoneLeft.localPosition = Vector3.zero;
            ikRigData.weaponBoneRight.localRotation = ikRigData.weaponBoneLeft.localRotation = Quaternion.identity;
            
            var additiveBone = new GameObject("WeaponBoneAdditive")
            {
                transform =
                {
                    parent = ikRigData.rootBone,
                    localRotation = Quaternion.identity,
                    localPosition = Vector3.zero
                }
            };
            ikRigData.weaponBoneAdditive = additiveBone.transform;
        }

        public void UpdateCoreComponent()
        {
            ikRigData.RetargetWeaponBone();
            animGraph.UpdateGraphWeights();
        }

        private void UpdateWeaponBone()
        {
            var spineRoot = ikRigData.spineRoot;
            var rootRot = ikRigData.rootBone.rotation;
            
            Quaternion hipsRotCached = ikRigData.pelvis.rotation;
            ikRigData.pelvis.rotation = rootRot * pelvisPoseMS;
            
            var spineRot = spineRoot.rotation;
            ikRigData.pelvis.rotation = hipsRotCached;
            
            spineRoot.rotation = Quaternion.Slerp(spineRoot.rotation, spineRot, animGraph.graphWeight);
            CoreToolkitLib.RotateInBoneSpace(rootRot, spineRoot, animGraph.GetSpineOffset(), 1f);

            // Parented to the right or left hand
            if (ikRigData.weaponBoneWeight > 0f)
            {
                LocRot basePose = weaponBonePose.FromSpace(ikRigData.rootBone);
                LocRot combinedPose = weaponBoneSpinePose.FromSpace(spineRoot);

                combinedPose.position -= basePose.position;
                combinedPose.rotation = Quaternion.Inverse(basePose.rotation) * combinedPose.rotation;
                
                ikRigData.weaponBone.position += combinedPose.position;
                ikRigData.weaponBone.rotation *= combinedPose.rotation;
            }
            
            ikRigData.masterDynamic.Retarget();
            ikRigData.UpdateWeaponParent();
            
            var rotOffset = weaponAsset != null ? weaponAsset.rotationOffset : Quaternion.identity;
            ikRigData.masterDynamic.Rotate(rotOffset, 1f);
            
            var pivotOffset = isPivotValid ? weaponTransformData.pivotPoint.localPosition : Vector3.zero;
            ikRigData.masterDynamic.Move(pivotOffset, 1f);
            
            ikRigData.rightHand.Retarget();
            ikRigData.leftHand.Retarget();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && (!_updateInEditor || !animGraph.IsPlaying()))
            {
                return;
            }
#endif
            onPreUpdate.Invoke();
            
            ikRigData.Retarget();
            UpdateWeaponBone();
            PreUpdateLayers();
            UpdateLayers();
            ApplyIK();
            PostUpdateLayers();
            
            var pivotOffset = isPivotValid ? weaponTransformData.pivotPoint.localPosition : Vector3.zero;
            ikRigData.AlignWeaponBone(-pivotOffset);
             
            onPostUpdate.Invoke();
        }

        private void OnDestroy()
        {
            onPreUpdate = onPostUpdate = null;
        }
        
        // Called right after retargeting
        private void PreUpdateLayers()
        {
            foreach (var layer in animLayers)
            {
                if (!Application.isPlaying && !layer.runInEditor)
                {
                    continue;
                }

                layer.OnPreAnimUpdate();
            }
        }

        private void UpdateLayers()
        {
            bool bValidElbows = ikRigData.rightHand.hintObj != null && ikRigData.leftHand.hintObj != null;
            if (bValidElbows)
            {
                var rightElbowIK = ikRigData.rightHand.hintObj.transform;
                var leftElbowIK = ikRigData.leftHand.hintObj.transform;

                rightElbowIK.position = ikRigData.rightHand.hintTarget.position;
                rightElbowIK.rotation = ikRigData.rightHand.hintTarget.rotation;
                
                leftElbowIK.position = ikRigData.leftHand.hintTarget.position;
                leftElbowIK.rotation = ikRigData.leftHand.hintTarget.rotation;
            }
            
            foreach (var layer in animLayers)
            {
                if (!Application.isPlaying && !layer.runInEditor)
                {
                    continue;
                }

                Transform rightElbow = null, leftElbow = null;
                LocRot cacheElbowRight = LocRot.identity, cacheElbowLeft = LocRot.identity;
                
                if (bValidElbows)
                {
                    rightElbow = ikRigData.rightHand.hintObj.transform;
                    leftElbow = ikRigData.leftHand.hintObj.transform;
                    
                    cacheElbowRight = new LocRot(rightElbow);
                    cacheElbowLeft = new LocRot(leftElbow);
                }
                
                layer.OnAnimUpdate();
                
                if (bValidElbows)
                {
                    float alpha = layer.elbowsWeight;
                    
                    rightElbow.position = Vector3.Lerp(cacheElbowRight.position,rightElbow.position, alpha);
                    rightElbow.rotation = Quaternion.Slerp(cacheElbowRight.rotation,rightElbow.rotation, alpha);
                    
                    leftElbow.position = Vector3.Lerp(cacheElbowLeft.position,leftElbow.position, alpha);
                    leftElbow.rotation = Quaternion.Slerp(cacheElbowLeft.rotation,leftElbow.rotation, alpha);
                }
            }
        }

        // Called after IK pass
        private void PostUpdateLayers()
        {
            foreach (var layer in animLayers)
            {
                if (!Application.isPlaying && !layer.runInEditor)
                {
                    continue;
                }

                layer.OnPostIK();
            }
        }
        
        // Called right before the pose sampling
        public void OnPrePoseSampled()
        {
            // Overwrite the weaponBone transform with the user data
            // Might be overwritten by the static pose after the pose is sampled
            LocRot target = new LocRot()
            {
                position = ikRigData.rootBone.TransformPoint(ikRigData.weaponTransform.position),
                rotation = ikRigData.rootBone.rotation * ikRigData.weaponTransform.rotation
            };

            ikRigData.weaponBone.position = target.position;
            ikRigData.weaponBone.rotation = target.rotation;
        }

        // Called after the pose is sampled
        public void OnPoseSampled()
        {
            ikRigData.RetargetHandBones();
            pelvisPoseMS = ikRigData.GetPelvisMS();
            
            weaponBonePose = new LocRot(ikRigData.weaponBone, false);
            weaponBoneSpinePose = new LocRot(ikRigData.weaponBone).ToSpace(ikRigData.spineRoot);
            
            foreach (var layer in animLayers)
            {
                if (!Application.isPlaying && !layer.runInEditor)
                {
                    continue;
                }

                layer.OnPoseSampled();
            }
        }

        public void OnGunEquipped(WeaponAnimAsset asset, WeaponTransformData data)
        {
            weaponAsset = asset;
            weaponTransformData = data;
            isPivotValid = weaponTransformData.pivotPoint != null;
        }

        public void OnSightChanged(Transform newSight)
        {
            weaponTransformData.aimPoint = newSight;
        }

        public void SetCharData(CharAnimData data)
        {
            characterData = data;
        }

        public void SetRightHandIKWeight(float effector, float hint)
        {
            rightHandWeight = Tuple.Create(effector, hint);
        }

        public void SetLeftHandIKWeight(float effector, float hint)
        {
            leftHandWeight = Tuple.Create(effector, hint);
        }

        public void SetRightFootIKWeight(float effector, float hint)
        {
            rightFootWeight = Tuple.Create(effector, hint);
        }

        public void SetLeftFootIKWeight(float effector, float hint)
        {
            leftFootWeight = Tuple.Create(effector, hint);
        }

// Editor utils
#if UNITY_EDITOR
        public void EnableEditorPreview()
        {
            if (ikRigData.animator == null)
            {
                ikRigData.animator = GetComponent<Animator>();
            }

            foreach (var layer in animLayers)
            {
                layer.OnEnable();
                layer.OnAnimStart();
            }

            animGraph.StartPreview();
            EditorApplication.QueuePlayerLoopUpdate();
            _updateInEditor = true;
        }

        public void DisableEditorPreview()
        {
            _updateInEditor = false;

            if (ikRigData.animator == null)
            {
                return;
            }

            animGraph.StopPreview();
            ikRigData.animator.Rebind();
            ikRigData.animator.Update(0f);

            if (ikRigData.tPose != null)
            {
                ikRigData.tPose.SampleAnimation(gameObject, 0f);
            }

            ikRigData.weaponBone.localPosition = Vector3.zero;
            ikRigData.weaponBone.localRotation = Quaternion.identity;
        }

        private void OnDrawGizmos()
        {
            if (drawDebug)
            {
                Gizmos.color = Color.green;

                void DrawDynamicBone(ref DynamicBone bone, string boneName)
                {
                    if (bone.obj != null)
                    {
                        var loc = bone.obj.transform.position;
                        Gizmos.DrawWireSphere(loc, 0.03f);
                        Handles.Label(loc, boneName);
                    }
                }

                DrawDynamicBone(ref ikRigData.rightHand, "RightHandIK");
                DrawDynamicBone(ref ikRigData.leftHand, "LeftHandIK");
                DrawDynamicBone(ref ikRigData.rightFoot, "RightFootIK");
                DrawDynamicBone(ref ikRigData.leftFoot, "LeftFootIK");

                Gizmos.color = Color.blue;
                if (ikRigData.rootBone != null)
                {
                    var mainBone = ikRigData.rootBone.position;
                    Gizmos.DrawWireCube(mainBone, new Vector3(0.1f, 0.1f, 0.1f));
                    Handles.Label(mainBone, "rootBone");
                }
            }
        }

        public void SetupBones()
        {
            if (ikRigData.animator == null)
            {
                ikRigData.animator = GetComponent<Animator>();
            }

            if (ikRigData.rootBone == null)
            {
                var root = transform.Find("rootBone");

                if (root != null)
                {
                    ikRigData.rootBone = root.transform;
                }
                else
                {
                    var bone = new GameObject("rootBone");
                    bone.transform.parent = transform;
                    ikRigData.rootBone = bone.transform;
                    ikRigData.rootBone.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.weaponBone == null)
            {
                var gunBone = ikRigData.rootBone.Find("WeaponBone");

                if (gunBone != null)
                {
                    ikRigData.weaponBone = gunBone.transform;
                }
                else
                {
                    var bone = new GameObject("WeaponBone");
                    bone.transform.parent = ikRigData.rootBone;
                    ikRigData.weaponBone = bone.transform;
                    ikRigData.weaponBone.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.rightFoot.obj == null)
            {
                var bone = transform.Find("RightFootIK");

                if (bone != null)
                {
                    ikRigData.rightFoot.obj = bone.gameObject;
                }
                else
                {
                    ikRigData.rightFoot.obj = new GameObject("RightFootIK");
                    ikRigData.rightFoot.obj.transform.parent = transform;
                    ikRigData.rightFoot.obj.transform.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.leftFoot.obj == null)
            {
                var bone = transform.Find("LeftFootIK");

                if (bone != null)
                {
                    ikRigData.leftFoot.obj = bone.gameObject;
                }
                else
                {
                    ikRigData.leftFoot.obj = new GameObject("LeftFootIK");
                    ikRigData.leftFoot.obj.transform.parent = transform;
                    ikRigData.leftFoot.obj.transform.localPosition = Vector3.zero;
                }
            }

            if (ikRigData.animator.isHuman)
            {
                ikRigData.pelvis = ikRigData.animator.GetBoneTransform(HumanBodyBones.Hips);
                ikRigData.spineRoot = ikRigData.animator.GetBoneTransform(HumanBodyBones.Spine);
                ikRigData.rightHand.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightHand);
                ikRigData.rightHand.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                ikRigData.leftHand.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftHand);
                ikRigData.leftHand.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                ikRigData.rightFoot.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightFoot);
                ikRigData.rightFoot.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                ikRigData.leftFoot.target = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                ikRigData.leftFoot.hintTarget = ikRigData.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);

                Transform head = ikRigData.animator.GetBoneTransform(HumanBodyBones.Head);
                SetupIKBones(head);
                SetupWeaponBones();
                return;
            }

            var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogWarning("Core: Skinned Mesh Renderer not found!");
                return;
            }

            var children = meshRenderer.bones;

            bool foundRightHand = false;
            bool foundLeftHand = false;
            bool foundRightFoot = false;
            bool foundLeftFoot = false;
            bool foundHead = false;
            bool foundPelvis = false;

            foreach (var bone in children)
            {
                if (bone.name.ToLower().Contains("ik"))
                {
                    continue;
                }

                bool bMatches = bone.name.ToLower().Contains("hips") || bone.name.ToLower().Contains("pelvis");
                if (!foundPelvis && bMatches)
                {
                    ikRigData.pelvis = bone;
                    foundPelvis = true;
                    continue;
                }

                bMatches = bone.name.ToLower().Contains("lefthand") || bone.name.ToLower().Contains("hand_l")
                                                                    || bone.name.ToLower().Contains("l_hand")
                                                                    || bone.name.ToLower().Contains("hand l")
                                                                    || bone.name.ToLower().Contains("l hand")
                                                                    || bone.name.ToLower().Contains("l.hand")
                                                                    || bone.name.ToLower().Contains("hand.l")
                                                                    || bone.name.ToLower().Contains("hand_left")
                                                                    || bone.name.ToLower().Contains("left_hand");
                if (!foundLeftHand && bMatches)
                {
                    ikRigData.leftHand.target = bone;

                    if (ikRigData.leftHand.hintTarget == null)
                    {
                        ikRigData.leftHand.hintTarget = bone.parent;
                    }
                    
                    foundLeftHand = true;
                    continue;
                }

                bMatches = bone.name.ToLower().Contains("righthand") || bone.name.ToLower().Contains("hand_r")
                                                                     || bone.name.ToLower().Contains("r_hand")
                                                                     || bone.name.ToLower().Contains("hand r")
                                                                     || bone.name.ToLower().Contains("r hand")
                                                                     || bone.name.ToLower().Contains("r.hand")
                                                                     || bone.name.ToLower().Contains("hand.r")
                                                                     || bone.name.ToLower().Contains("hand_right")
                                                                     || bone.name.ToLower().Contains("right_hand");
                if (!foundRightHand && bMatches)
                {
                    ikRigData.rightHand.target = bone;

                    if (ikRigData.rightHand.hintTarget == null)
                    {
                        ikRigData.rightHand.hintTarget = bone.parent;
                    }
                    
                    foundRightHand = true;
                }

                bMatches = bone.name.ToLower().Contains("rightfoot") || bone.name.ToLower().Contains("foot_r")
                                                                     || bone.name.ToLower().Contains("r_foot")
                                                                     || bone.name.ToLower().Contains("foot_right")
                                                                     || bone.name.ToLower().Contains("right_foot")
                                                                     || bone.name.ToLower().Contains("foot r")
                                                                     || bone.name.ToLower().Contains("r foot")
                                                                     || bone.name.ToLower().Contains("r.foot")
                                                                     || bone.name.ToLower().Contains("foot.r");
                if (!foundRightFoot && bMatches)
                {
                    ikRigData.rightFoot.target = bone;
                    ikRigData.rightFoot.hintTarget = bone.parent;

                    foundRightFoot = true;
                }

                bMatches = bone.name.ToLower().Contains("leftfoot") || bone.name.ToLower().Contains("foot_l")
                                                                    || bone.name.ToLower().Contains("l_foot")
                                                                    || bone.name.ToLower().Contains("foot l")
                                                                    || bone.name.ToLower().Contains("foot_left")
                                                                    || bone.name.ToLower().Contains("left_foot")
                                                                    || bone.name.ToLower().Contains("l foot")
                                                                    || bone.name.ToLower().Contains("l.foot")
                                                                    || bone.name.ToLower().Contains("foot.l");
                if (!foundLeftFoot && bMatches)
                {
                    ikRigData.leftFoot.target = bone;
                    ikRigData.leftFoot.hintTarget = bone.parent;

                    foundLeftFoot = true;
                }

                if (!foundHead && bone.name.ToLower().Contains("head"))
                {
                    SetupIKBones(bone);
                    foundHead = true;
                }
            }

            SetupWeaponBones();
            
            bool bFound = foundRightHand && foundLeftHand && foundRightFoot && foundLeftFoot && foundHead &&
                          foundPelvis;

            Debug.Log(bFound ? "All bones are found!" : "Some bones are missing!");
        }

        private void SetupIKBones(Transform head)
        {
            if (ikRigData.masterDynamic.obj == null)
            {
                var boneObject = head.transform.Find("MasterIK");

                if (boneObject != null)
                {
                    ikRigData.masterDynamic.obj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.masterDynamic.obj = new GameObject("MasterIK");
                    ikRigData.masterDynamic.obj.transform.parent = head;
                    ikRigData.masterDynamic.obj.transform.localPosition = Vector3.zero;
                }
            }
            
            ikRigData.masterDynamic.target = ikRigData.weaponBone;

            if (ikRigData.rightHand.obj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("RightHandIK");

                if (boneObject != null)
                {
                    ikRigData.rightHand.obj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.rightHand.obj = new GameObject("RightHandIK");
                }

                ikRigData.rightHand.obj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.rightHand.obj.transform.localPosition = Vector3.zero;
            }

            if (ikRigData.rightHand.hintObj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("RightElbowIK");

                if (boneObject != null)
                {
                    ikRigData.rightHand.hintObj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.rightHand.hintObj = new GameObject("RightElbowIK");
                }

                ikRigData.rightHand.hintObj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.rightHand.hintObj.transform.localPosition = Vector3.zero;
            }
            
            if (ikRigData.leftHand.obj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("LeftHandIK");

                if (boneObject != null)
                {
                    ikRigData.leftHand.obj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.leftHand.obj = new GameObject("LeftHandIK");
                }

                ikRigData.leftHand.obj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.leftHand.obj.transform.localPosition = Vector3.zero;
            }
            
            if (ikRigData.leftHand.hintObj == null)
            {
                var boneObject = ikRigData.masterDynamic.obj.transform.Find("LeftElbowIK");

                if (boneObject != null)
                {
                    ikRigData.leftHand.hintObj = boneObject.gameObject;
                }
                else
                {
                    ikRigData.leftHand.hintObj = new GameObject("LeftElbowIK");
                }

                ikRigData.leftHand.hintObj.transform.parent = ikRigData.masterDynamic.obj.transform;
                ikRigData.leftHand.hintObj.transform.localPosition = Vector3.zero;
            }
        }

        private void SetupWeaponBones()
        {
            var rightHand = ikRigData.rightHand.target;
            var lefTHand= ikRigData.leftHand.target;
            
            if (rightHand != null && ikRigData.weaponBoneRight == null)
            {
                var boneObject = rightHand.Find("WeaponBoneRight");

                if (boneObject == null)
                {
                    var weaponBone = new GameObject("WeaponBoneRight");
                    ikRigData.weaponBoneRight = weaponBone.transform;
                    ikRigData.weaponBoneRight.parent = rightHand;
                }
                else
                {
                    ikRigData.weaponBoneRight = boneObject;
                }
            }
            
            if (lefTHand != null && ikRigData.weaponBoneLeft == null)
            {
                var boneObject = lefTHand.Find("WeaponBoneLeft");

                if (boneObject == null)
                {
                    var weaponBone = new GameObject("WeaponBoneLeft");
                    ikRigData.weaponBoneLeft = weaponBone.transform;
                    ikRigData.weaponBoneLeft.parent = lefTHand;
                }
                else
                {
                    ikRigData.weaponBoneLeft = boneObject;
                }
            }
        }

        public void AddLayer(AnimLayer newLayer)
        {
            animLayers.Add(newLayer);
        }

        public void RemoveLayer(int index)
        {
            if (index < 0 || index > animLayers.Count - 1)
            {
                return;
            }

            var toRemove = animLayers[index];
            animLayers.RemoveAt(index);
            DestroyImmediate(toRemove, true);
        }

        public bool IsLayerUnique(Type layer)
        {
            bool isUnique = true;
            foreach (var item in animLayers)
            {
                if (item.GetType() == layer)
                {
                    isUnique = false;
                    break;
                }
            }

            return isUnique;
        }

        public AnimLayer GetLayer(int index)
        {
            if (index < 0 || index > animLayers.Count - 1)
            {
                return null;
            }

            return animLayers[index];
        }

        public bool HasA(AnimLayer item)
        {
            return animLayers.Contains(item);
        }
#endif
    }
}