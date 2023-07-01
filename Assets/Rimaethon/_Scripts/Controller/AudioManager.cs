using System;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Rimaethon._Scripts.Controller
{
    public class AudioManager: StaticInstance<AudioManager>
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource soundEffectsSource;
        [SerializeField] private List<AudioClip> backgroundMusicClips;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundEffectsVolumeSlider;

        private void OnEnable()
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            soundEffectsVolumeSlider.onValueChanged.AddListener(OnSoundEffectsVolumeChanged);
        }

        private void OnDisable()
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            soundEffectsVolumeSlider.onValueChanged.RemoveListener(OnSoundEffectsVolumeChanged);
        }

        private void Start()
        {
            musicSource.volume = musicVolumeSlider.value;
            soundEffectsSource.volume = soundEffectsVolumeSlider.value;
           
            PlayRandomBackgroundMusic();
        }

        private void OnMusicVolumeChanged(float volume)
        {
            musicSource.volume = volume;
        }

        private void OnSoundEffectsVolumeChanged(float volume)
        {
            soundEffectsSource.volume = volume;
        }

        private void PlayMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlaySoundEffect(AudioClip clip,Vector3 pos)
        {
            soundEffectsSource.transform.position= pos;
            soundEffectsSource.PlayOneShot(clip, soundEffectsSource.volume);
        }

        private void PlayRandomBackgroundMusic()
        {
            if (backgroundMusicClips.Count > 0)
            {
                int randomIndex = Random.Range(0, backgroundMusicClips.Count);
                AudioClip randomClip = backgroundMusicClips[randomIndex];
                PlayMusic(randomClip);
            }
        }
    }
}