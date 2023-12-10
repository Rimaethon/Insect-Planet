// Designed by KINEMATION, 2023

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [Serializable, Tooltip("Blend time in seconds")]
    public struct BlendTime
    {
        [Min(0f)] public float blendInTime;
        [Min(0f)] public float blendOutTime;
        [Min(0f)] public float rateScale;

        public BlendTime(float blendIn, float blendOut)
        {
            blendInTime = blendIn;
            blendOutTime = blendOut;
            rateScale = 1f;
        }

        public void Validate()
        {
            blendInTime = blendInTime < 0f ? 0f : blendInTime;
            blendOutTime = blendOutTime < 0f ? 0f : blendOutTime;
        }
    }

    public struct AnimTime
    {
        public BlendTime blendTime;
        public float startTime;

        public AnimTime(float blendIn, float blendOut, float startTime = 0f)
        {
            blendTime = new BlendTime(blendIn, blendOut);
            this.startTime = startTime;
        }
    }

    public struct CoreAnimPlayable
    {
        public AnimationClipPlayable playableClip;
        public AnimTime animTime;
        public float cachedWeight;

        public CoreAnimPlayable(PlayableGraph graph, AnimationClip clip)
        {
            playableClip = AnimationClipPlayable.Create(graph, clip);
            animTime = new AnimTime(0f, 0f);
            cachedWeight = 0f;
        }
        
        public float GetLength()
        {
            return playableClip.IsValid() ? playableClip.GetAnimationClip().length : 0f;
        }

        public void Release()
        {
            if (playableClip.IsValid())
            {
                animTime = new AnimTime(0f, 0f);
                playableClip.Destroy();
            }
        }
    }
    
    public struct CoreAnimMixer
    {
        public AnimationLayerMixerPlayable mixer;
        public float blendInWeight;
        public float blendOutWeight;
        
        private List<CoreAnimPlayable> _playables;
        private float _mixerWeight;
        private float _playingWeight;
        private int _playingIndex;
        private bool _bBlendOut;
        private bool _bForceBlendOut;
        private float _forceBlendTime;
        private float _forceStartBlendTime;
        
        private AnimCurve[] _curves;
        private Dictionary<string, AnimCurveValue> _curveTable;
        private List<string> _inActiveCurves;
        
        public CoreAnimMixer(PlayableGraph graph, int inputCount, bool bBlendOut)
        {
            mixer = AnimationLayerMixerPlayable.Create(graph, inputCount);
            
            _playables = new List<CoreAnimPlayable>();
            for (int i = 0; i < inputCount - 1; i++)
            {
                _playables.Add(new CoreAnimPlayable());
            }
            
            _bBlendOut = bBlendOut;
            _playingIndex = -1;
            _mixerWeight = 1f;
            _playingWeight = blendInWeight = blendOutWeight = 0f;
            _curves = null;
            _curveTable = new Dictionary<string, AnimCurveValue>();
            _inActiveCurves = new List<string>();
            _bForceBlendOut = false;
            _forceBlendTime = _forceStartBlendTime = 0f;
        }

        public void OnSampleUpdate(AvatarMask mask, bool bUpdateWeights = true)
        {
            for (int i = 1; i <= _playingIndex; i++)
            {
                if (!mixer.GetInput(i).IsValid())
                {
                    continue;
                }
                
                mixer.SetLayerMaskFromAvatarMask((uint) i, mask);

                if (bUpdateWeights)
                {
                    float weight = mixer.GetInputWeight(i);
                    mixer.SetInputWeight(i, weight * _mixerWeight);
                }
            }
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

        public void AddClip(CoreAnimPlayable clip, AvatarMask mask, AnimCurve[] curves = null)
        {
            CacheCurves();
            _curves = curves;
            AddCurves();
            
            UpdatePlayingIndex();
            clip.animTime.blendTime.Validate();
            
            mixer.ConnectInput(_playingIndex, clip.playableClip, 0, 0);
            _playables[_playingIndex - 1] = clip;
            mixer.SetLayerMaskFromAvatarMask((uint) _playingIndex, mask);
            _bForceBlendOut = false;

            blendOutWeight = 0f;
        }

        public void Stop(float blendOutTime)
        {
            if (!mixer.GetInput(_playingIndex).IsValid())
            {
                return;
            }
            
            _forceBlendTime = blendOutTime;
            _forceStartBlendTime = (float) _playables[_playingIndex - 1].playableClip.GetTime();
            _bForceBlendOut = true;
        }

        public float GetCurveValue(string curveName)
        {
            if (!_curveTable.ContainsKey(curveName)) return 0f;
            return _curveTable[curveName].value;
        }

        public float Update()
        {
            if (!mixer.GetInput(_playingIndex).IsValid())
            {
                return 0f;
            }

            if (_bForceBlendOut)
            {
                ForceBlendOut();
            }
            else
            {
                BlendInPlayable();
                BlendOutPlayable();
            }
            
            return _playingWeight;
        }

        public void UpdateMixerWeight()
        {
            BlendMixerWeight();
        }
        
        public void SetMixerWeight(float weight)
        {
            _mixerWeight = Mathf.Clamp01(weight);
        }

        private void BlendMixerWeight()
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

        // Save curve values
        private void CacheCurves()
        {
            if (_curves == null) return;
            
            foreach (var curve in _curves)
            {
                var newCurve = _curveTable[curve.name];
                newCurve.cache = newCurve.value;
                _curveTable[curve.name] = newCurve;
            }
        }

        private void AddCurves()
        {
            if (_curves == null) return;
            
            _inActiveCurves.Clear();
            foreach (var curve in _curves)
            {
                if (!_curveTable.ContainsKey(curve.name))
                {
                    _curveTable.Add(curve.name, new AnimCurveValue());
                }
            }

            var activeCurveNames = new HashSet<string>(_curves.Select(c => c.name));
            foreach (var curve in _curveTable)
            {
                if (!activeCurveNames.Contains(curve.Key))
                {
                    _inActiveCurves.Add(curve.Key);
                }
            }
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

        private void UpdateCurve(string curveName, float value)
        {
            var newCurve = _curveTable[curveName];
            newCurve.target = value;
            _curveTable[curveName] = newCurve;
        }

        private void BlendInCurve(string curveName, float weight)
        {
            var newCurve = _curveTable[curveName];
            newCurve.value = Mathf.Lerp(newCurve.cache, newCurve.target, weight);
            _curveTable[curveName] = newCurve;
        }
        
        private void BlendOutCurve(string curveName, float weight)
        {
            var newCurve = _curveTable[curveName];
            newCurve.value *= 1f - weight;
            _curveTable[curveName] = newCurve;
        }

        private void BlendInPlayable()
        {
            var animation = _playables[_playingIndex - 1];
            float blendTime = animation.animTime.blendTime.blendInTime;
            var time = (float) animation.playableClip.GetTime();
            
            if (_bBlendOut && (time >= animation.GetLength()))
            {
                return;
            }
            
            // todo: use CurveLib easing functions
            float alpha = Mathf.Approximately(blendTime, 0f) ? 1f : (time - animation.animTime.startTime) / blendTime;
            _playingWeight = Mathf.Lerp(0f, 1f, alpha);
            blendInWeight = _playingWeight; 
            mixer.SetInputWeight(_playingIndex, _playingWeight);
            
            BlendOutInactive();

            // Blend out inactive curves.
            foreach (var curve in _inActiveCurves)
            {
                BlendOutCurve(curve, 1 - _playingWeight);
            }

            if (Mathf.Approximately(1f, _playingWeight))
            {
                _inActiveCurves.Clear();
            }

            if (_curves == null) return;
            
            //Blend curves here
            foreach (var curve in _curves)
            {
                float curveValue = curve.curve != null ? curve.curve.Evaluate(time / animation.GetLength()) : 0f;
                UpdateCurve(curve.name, curveValue);
                BlendInCurve(curve.name, _playingWeight);
            }
        }

        private void BlendOutPlayable()
        {
            if (!_bBlendOut)
            {
                return;
            }

            var animPlayable = _playables[_playingIndex - 1];
            var animTime = animPlayable.animTime;
            var time = (float) animPlayable.playableClip.GetTime();
            
            if (time >= animPlayable.GetLength())
            {
                // todo: use CurveLib ease functions
                float alpha = 0f;
                if (Mathf.Approximately(animTime.blendTime.blendOutTime, 0f))
                {
                    alpha = 1f;
                }
                else
                {
                    alpha = (time - animPlayable.GetLength()) / animTime.blendTime.blendOutTime;
                }
                
                float weight = Mathf.Lerp(_playingWeight, 0f, alpha);
                mixer.SetInputWeight(_playingIndex, weight);
                blendOutWeight = alpha;

                if (Mathf.Approximately(weight, 0f))
                {
                    mixer.DisconnectInput(_playingIndex);
                    _playables[_playingIndex - 1].Release();
                    _playingIndex = -1;
                    blendOutWeight = 1f;
                    alpha = 1f;
                }
                
                // Blend out curves here
                foreach (var curve in _curves)
                {
                    BlendOutCurve(curve.name, alpha);
                }

                foreach (var inActiveCurve in _inActiveCurves)
                {
                    BlendOutCurve(inActiveCurve, weight);
                }
            }
        }

        private void ForceBlendOut()
        {
            var animPlayable = _playables[_playingIndex - 1];
            var time = (float) animPlayable.playableClip.GetTime();

            //todo: check the zero case
            float outWeight = (time - _forceStartBlendTime) / _forceBlendTime;
            outWeight = Mathf.Clamp01(outWeight);
            
            for (int i = 1; i <= _playingIndex; i++)
            {
                var animation = _playables[i - 1];
                if (!animation.playableClip.IsValid())
                {
                    continue;
                }

                _playingWeight *= 1f - outWeight;
                mixer.SetInputWeight(i, mixer.GetInputWeight(i) * (1f - outWeight));
                blendOutWeight = 1f - outWeight;
                
                if (Mathf.Approximately(outWeight, 1f))
                {
                    mixer.DisconnectInput(i);
                    _playables[i - 1].Release();
                }
            }

            foreach (var curve in _curves)
            {
                BlendOutCurve(curve.name, outWeight);
            }

            foreach (var curve in _inActiveCurves)
            {
                BlendOutCurve(curve, outWeight);
            }

            if (Mathf.Approximately(outWeight, 1f))
            {
                _inActiveCurves.Clear();
            }
        }

        private void BlendOutInactive()
        {
            if (!_bBlendOut)
            {
                return;
            }
            
            for (int i = 1; i < _playingIndex; i++)
            {
                var animation = _playables[i - 1];
                if (!animation.playableClip.IsValid())
                {
                    continue;
                }

                float weight = Mathf.Lerp(animation.cachedWeight, 0f, _playingWeight);
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