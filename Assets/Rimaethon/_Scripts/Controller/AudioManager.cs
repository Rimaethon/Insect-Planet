using System;
using System.Collections;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using AYellowpaper.SerializedCollections;

namespace Rimaethon._Scripts.Controller
{
    public class AudioManager: PersistentSingleton<AudioManager>
    {
        private AudioSource musicSource;
        [SerializeField] private float fadeTime = 0.5f; 
        [SerializeField] private AudioSource soundEffectsSource;
        [SerializeField] private List<AudioClip> backgroundMusicClips;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundEffectsVolumeSlider;
        public AYellowpaper.SerializedCollections.SerializedDictionary<string,AudioClip> soundEffectsClips;
        

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

        public void PlayMusic(AudioClip clip)
        {
            StartCoroutine(FadeOutAndPlayNewClip(clip));
        }

        private IEnumerator FadeOutAndPlayNewClip(AudioClip newClip)
        {
            yield return StartCoroutine(FadeOut());
            musicSource.clip = newClip;
            yield return StartCoroutine(FadeIn());
        }

        private IEnumerator FadeOut()
        {
            float startVolume = musicSource.volume;

            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume; // Resetting for future uses
        }

        private IEnumerator FadeIn()
        {
            float targetVolume = musicSource.volume;
            musicSource.volume = 0.0f; // Start from silence
            musicSource.Play();

            while (musicSource.volume < targetVolume)
            {
                musicSource.volume += targetVolume * Time.deltaTime / fadeTime;
                yield return null;
            }
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