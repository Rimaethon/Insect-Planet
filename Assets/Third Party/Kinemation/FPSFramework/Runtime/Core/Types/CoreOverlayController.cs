// Designed by KINEMATION, 2023

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    public struct CoreOverlayController
    {
        public AnimatorControllerPlayable controllerPlayable;
        public float blendTime;
        public float cachedWeight;

        public CoreOverlayController(PlayableGraph graph, RuntimeAnimatorController controller)
        {
            controllerPlayable = AnimatorControllerPlayable.Create(graph, controller);
            blendTime = cachedWeight = 0f;
        }

        public void Release()
        {
            if (controllerPlayable.IsValid())
            {
                blendTime = 0f;
                controllerPlayable.Destroy();
            }
        }
    }
    
    public struct CoreOverlayMixer
    {
        public AnimationLayerMixerPlayable mixer;
        
        private List<CoreOverlayController> _playables;
        private float _mixerWeight;
        private float _playingWeight;
        private int _playingIndex;

        public CoreOverlayMixer(PlayableGraph graph, int inputCount)
        {
            mixer = AnimationLayerMixerPlayable.Create(graph, inputCount);
            
            _playables = new List<CoreOverlayController>();
            for (int i = 0; i < inputCount - 1; i++)
            {
                _playables.Add(new CoreOverlayController());
            }
            
            _playingIndex = -1;
            _mixerWeight = 1f;
            _playingWeight = 0f;
        }
        
        public float Update()
        {
            if (!mixer.GetInput(_playingIndex).IsValid())
            {
                return 0f;
            }
            
            BlendInController();

            return _playingWeight;
        }
        
        public void SetAvatarMask(AvatarMask mask)
        {
            for (int i = 1; i <= _playingIndex; i++)
            {
                if (mixer.GetInput(i).IsValid())
                {
                    mixer.SetLayerMaskFromAvatarMask((uint) i, mask);
                }
            }
        }

        public void AddController(CoreOverlayController controller, AvatarMask mask)
        {
            UpdatePlayingIndex();
            controller.blendTime = Mathf.Max(controller.blendTime, 0f);
            
            mixer.ConnectInput(_playingIndex, controller.controllerPlayable, 0, 0f);
            _playables[_playingIndex - 1] = controller;
            mixer.SetLayerMaskFromAvatarMask((uint) _playingIndex, mask);
        }
        
        public void UpdateMixerWeight()
        {
            for (int i = 1; i <= _playingIndex; i++)
            {
                if (!mixer.GetInput(i).IsValid())
                {
                    continue;
                }

                float weight = mixer.GetInputWeight(i);
                mixer.SetInputWeight(i, weight * _mixerWeight);
            }
        }
        
        public void SetMixerWeight(float weight)
        {
            _mixerWeight = Mathf.Clamp01(weight);
        }

        private void UpdatePlayingIndex()
        {
            if (_playingIndex == -1)
            {
                for (int i = 1; i < mixer.GetInputCount(); i++)
                {
                    mixer.DisconnectInput(i);
                    _playables[i - 1].Release();
                }
                _playingIndex = 1;
                return;
            }
            
            // Try to use the next slot
            if (_playingIndex + 1 < mixer.GetInputCount())
            {
                _playingIndex++;
                // Save current weights
                for (int i = 1; i < _playingIndex; i++)
                {
                    var clip = _playables[i - 1];
                    clip.cachedWeight = mixer.GetInputWeight(i);
                    _playables[i - 1] = clip;
                }
                return;
            }

            _playables[0].Release();
            // Reconnect
            for (int i = 1; i < mixer.GetInputCount() - 1; i++)
            {
                if (!mixer.GetInput(i + 1).IsValid())
                {
                    continue;
                }
                
                float inputWeight = mixer.GetInputWeight(i + 1);
                var clip = _playables[i];
                clip.cachedWeight = inputWeight;
                _playables[i - 1] = clip;

                mixer.DisconnectInput(i);
                var source = mixer.GetInput(i + 1);
                mixer.DisconnectInput(i + 1);
                mixer.ConnectInput(i, source, 0, inputWeight);
            }
            
            _playingIndex = mixer.GetInputCount() - 1;
            mixer.DisconnectInput(_playingIndex);
        }
        
        private void BlendInController()
        {
            var controller = _playables[_playingIndex - 1];
            float blendTime = controller.blendTime;
            var time = (float) controller.controllerPlayable.GetTime();
            
            // todo: use CurveLib easing functions
            float alpha = Mathf.Approximately(blendTime, 0f) ? 1f : time / blendTime;
            _playingWeight = Mathf.Lerp(0f, 1f, alpha);
            mixer.SetInputWeight(_playingIndex, _playingWeight);
            
            BlendOutInactive();
        }
        
        private void BlendOutInactive()
        {
            for (int i = 1; i < _playingIndex; i++)
            {
                var controller = _playables[i - 1];
                if (!controller.controllerPlayable.IsValid())
                {
                    continue;
                }

                float weight = Mathf.Lerp(controller.cachedWeight, 0f, _playingWeight);
                mixer.SetInputWeight(i, weight);
                
                if (Mathf.Approximately(weight, 0f))
                {
                    mixer.DisconnectInput(i);
                    _playables[i - 1].Release();
                }
            }
        }
    }
}