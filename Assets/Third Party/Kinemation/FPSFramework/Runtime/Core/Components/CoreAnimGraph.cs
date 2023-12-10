// Designed by KINEMATION, 2023

using System;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Kinemation.FPSFramework.Runtime.Core.Components
{
    // Unity Animator sub-system
    [ExecuteInEditMode, Serializable]
    public class CoreAnimGraph : MonoBehaviour
    {
        public float graphWeight;
        [SerializeField] private AvatarMask upperBodyMask;

        [Tooltip("Max blending poses")] private int maxPoseCount = 3;
        [Tooltip("Max blending clips")] private int maxAnimCount = 3;

        private Animator _animator;
        private PlayableGraph _playableGraph;

        //private CoreOverlayMixer _overlayMixer;
        private CoreAnimMixer _overlayPoseMixer;
        private CoreAnimMixer _slotAnimMixer;
        private AnimationLayerMixerPlayable _masterMixer;
        
        private float _poseProgress = 0f;

        private Quaternion outSpineRot = Quaternion.identity;
        private Quaternion targetSpineRot = Quaternion.identity;
        private Quaternion cacheSpineRot = Quaternion.identity;

#if UNITY_EDITOR
        [SerializeField] [HideInInspector] private AnimationClip previewClip;
        [SerializeField] [HideInInspector] private bool loopPreview;
#endif

        public bool InitPlayableGraph()
        {
            if (_playableGraph.IsValid())
            {
                return true;
            }
            
            _animator = GetComponent<Animator>();
            _playableGraph = _animator.playableGraph;

            if (!_playableGraph.IsValid())
            {
                Debug.LogWarning(gameObject.name + " Animator Controller is null!");
                return false;
            }
            
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            
            _masterMixer = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
            _slotAnimMixer = new CoreAnimMixer(_playableGraph, 1 + maxAnimCount, true);
            var output = AnimationPlayableOutput.Create(_playableGraph, "FPSAnimator", _animator);

            _overlayPoseMixer = new CoreAnimMixer(_playableGraph, 1 + maxPoseCount, false);
            //_overlayMixer = new CoreOverlayMixer(_playableGraph, 1 + maxPoseCount);

            var controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);
            
            _playableGraph.Connect(controllerPlayable, 0, _overlayPoseMixer.mixer, 0);
            _playableGraph.Connect(_overlayPoseMixer.mixer, 0, _slotAnimMixer.mixer,0);
            _playableGraph.Connect(_slotAnimMixer.mixer, 0, _masterMixer, 0);

            // Enable Animator layer by default
            _overlayPoseMixer.mixer.SetInputWeight(0,1f);
            _slotAnimMixer.mixer.SetInputWeight(0 ,1f);
            _masterMixer.SetInputWeight(0, 1f);
            
            output.SetSourcePlayable(_masterMixer);
            
            _playableGraph.Play();
            return true;
        }
        
        public void UpdateGraph()
        {
            if (Application.isPlaying)
            {
                _poseProgress = _overlayPoseMixer.Update();
                _slotAnimMixer.Update();
                outSpineRot = Quaternion.Slerp(cacheSpineRot, targetSpineRot, _slotAnimMixer.blendInWeight);
                outSpineRot = Quaternion.Slerp(outSpineRot, Quaternion.identity, _slotAnimMixer.blendOutWeight);
            }
        }

        public Quaternion GetSpineOffset()
        {
            return outSpineRot;
        }
        
        public float GetCurveValue(string curveName)
        {
            return _slotAnimMixer.GetCurveValue(curveName);
        }

        public float GetPoseProgress()
        {
            return _poseProgress;
        }

        public void SetGraphWeight(float weight)
        {
            if (!_playableGraph.IsValid())
            {
                return;
            }

            graphWeight = weight;
            _overlayPoseMixer.SetMixerWeight(weight);
            _slotAnimMixer.SetMixerWeight(weight);
        }
        
        //todo: implement custom animator controllers
        public void PlayController(RuntimeAnimatorController controller, AnimationClip clip, float blendTime)
        {
            if (controller == null)
            {
                return;
            }
            
            CoreOverlayController controllerPlayable = new CoreOverlayController(_playableGraph, controller)
            {
                blendTime = blendTime
            };

            controllerPlayable.controllerPlayable.SetTime(0f);
            //_overlayPoseMixer.AddController(controllerPlayable, upperBodyMask);
            
            SamplePose(clip);
        }
        
        public void PlayPose(AnimationClip clip, float blendIn)
        {
            if (clip == null)
            {
                return;
            }
            
            CoreAnimPlayable animPlayable = new CoreAnimPlayable(_playableGraph, clip)
            {
                animTime = new AnimTime(blendIn, 0f)
            };

            animPlayable.playableClip.SetTime(0f);
            animPlayable.playableClip.SetSpeed(1f);
            _overlayPoseMixer.AddClip(animPlayable, upperBodyMask);

            SamplePose(clip);
        }
        
        public void PlayAnimation(AnimationClip clip, AnimTime animTime, Quaternion spineRot, 
            AnimCurve[] curves = null)
        {
            if (clip == null)
            {
                return;
            }

            cacheSpineRot = outSpineRot;
            targetSpineRot = spineRot;

            CoreAnimPlayable animPlayable = new CoreAnimPlayable(_playableGraph, clip)
            {
                animTime = animTime
            };

            animPlayable.playableClip.SetTime(animTime.startTime);
            animPlayable.playableClip.SetSpeed(animTime.blendTime.rateScale);
            _slotAnimMixer.AddClip(animPlayable, upperBodyMask, curves);
        }

        public void StopAnimation(float blendTime)
        {
            _slotAnimMixer.Stop(blendTime);
        }

        public bool IsPlaying()
        {
            return _playableGraph.IsValid() && _playableGraph.IsPlaying();
        }

        public void UpdateGraphWeights()
        {
            _overlayPoseMixer.UpdateMixerWeight();
            _slotAnimMixer.UpdateMixerWeight();
        }
        
        // Samples overlay static pose, must be called during Update()
        public void SamplePose(AnimationClip clip)
        {
            clip.SampleAnimation(transform.gameObject, 0f);
        }

        public AvatarMask GetUpperBodyMask()
        {
            return upperBodyMask;
        }
        
        private void OnDestroy()
        {
            if (!_playableGraph.IsValid())
            {
                return;
            }

            _playableGraph.Stop();
            _playableGraph.Destroy();
        }

#if UNITY_EDITOR
        private void LoopPreview()
        {
            if (!_playableGraph.IsPlaying())
            {
                EditorApplication.update -= LoopPreview;
            }
            
            if (loopPreview && _playableGraph.IsValid() 
                            && _masterMixer.GetInput(1).GetTime() >= previewClip.length)
            {
                _masterMixer.GetInput(1).SetTime(0f);
            }
        }
        
        public void StartPreview()
        {
            if (!InitPlayableGraph())
            {
                return;
            }

            if (previewClip != null)
            {
                var previewPlayable = AnimationClipPlayable.Create(_playableGraph, previewClip);
                previewPlayable.SetTime(0f);
                previewPlayable.SetSpeed(1f);

                if (_masterMixer.GetInput(1).IsValid())
                {
                    _masterMixer.DisconnectInput(1);
                }

                _masterMixer.ConnectInput(1, previewPlayable, 0, 1f);
                EditorApplication.update += LoopPreview;
            }

            _playableGraph.Play();
        }

        public void StopPreview()
        {
            if (_playableGraph.IsValid())
            {
                _masterMixer.SetInputWeight(1, 0f);
                _masterMixer.DisconnectInput(1);
                _playableGraph.Stop();
            }
            
            EditorApplication.update -= LoopPreview;
        }
#endif
    }
}