using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Core
{
    [RequireComponent(typeof(AudioSourcePool))]
    public class AudioManager : MonoBehaviour, ISoundPlayer
    {
        public static AudioManager Instance { get; private set; }

        private readonly Dictionary<SoundEffectSO, AudioSource> activeLoopingSounds = new();
        private AudioSourcePool audioSourcePool;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                audioSourcePool = GetComponent<AudioSourcePool>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySound(SoundEffectSO sound)
        {
            if (sound == null || sound.clip == null) return;

            if (sound.loop)
                PlayLoopingSound(sound);
            else
                PlayOneShotSound(sound);
        }

        private void PlayLoopingSound(SoundEffectSO sound)
        {
            if (sound.interruptLoopOnReplay)
            {
                if (activeLoopingSounds.TryGetValue(sound, out var existingSource) && existingSource != null)
                    StopAndReturn(existingSource);
            }
            else if (activeLoopingSounds.ContainsKey(sound))
            {
                return;
            }

            var audioSource = audioSourcePool.GetAudioSource();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.loop = true;
            audioSource.pitch = Random.Range(sound.pitchMin, sound.pitchMax);
            audioSource.outputAudioMixerGroup = sound.mixerGroup;

            if (sound.fadeIn)
                FadeIn(audioSource, sound.volume, sound.fadeInTime);
            else
                audioSource.Play();

            activeLoopingSounds[sound] = audioSource;
        }

        private void PlayOneShotSound(SoundEffectSO sound)
        {
            var audioSource = audioSourcePool.GetAudioSource();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.loop = false;
            audioSource.pitch = Random.Range(sound.pitchMin, sound.pitchMax);
            audioSource.outputAudioMixerGroup = sound.mixerGroup;
            audioSource.Play();

            StartCoroutine(ReturnAudioSourceAfterClip(audioSource, sound.clip.length));
        }

        private IEnumerator ReturnAudioSourceAfterClip(AudioSource audioSource, float clipLength)
        {
            yield return new WaitForSeconds(clipLength);

            if (audioSource != null && audioSourcePool != null)
                audioSourcePool.ReturnAudioSource(audioSource);
        }

        public void StopLoopingSound(SoundEffectSO sound)
        {
            if (activeLoopingSounds.TryGetValue(sound, out var audioSource))
            {
                if (audioSource != null)
                {
                    if (sound.fadeOut)
                        FadeOutAndReturnToPool(audioSource, sound.fadeOutTime);
                    else
                        StopAndReturn(audioSource);
                }

                activeLoopingSounds.Remove(sound);
            }
        }

        public void StopAllLoopingSounds()
        {
            foreach (var audioSource in activeLoopingSounds.Values)
            {
                if (audioSource != null)
                    StopAndReturn(audioSource);
            }

            activeLoopingSounds.Clear();
        }

        private void StopAndReturn(AudioSource source)
        {
            source.Stop();
            audioSourcePool.ReturnAudioSource(source);
        }

        private void FadeIn(AudioSource audioSource, float targetVolume, float duration)
        {
            audioSource.volume = 0;
            audioSource.Play();
            audioSource.DOKill();
            audioSource.DOFade(targetVolume, duration);
        }

        private void FadeOutAndReturnToPool(AudioSource audioSource, float duration)
        {
            audioSource.DOKill();
            audioSource.DOFade(0, duration).OnComplete(() =>
            {
                audioSource.Stop();
                audioSourcePool.ReturnAudioSource(audioSource);
            });
        }
    }
}
