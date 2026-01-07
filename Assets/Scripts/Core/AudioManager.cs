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

        private readonly Dictionary<SoundEffectSO, AudioSource> _activeLoopingSounds = new();
        private AudioSourcePool _audioSourcePool;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _audioSourcePool = GetComponent<AudioSourcePool>();
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
            if (_activeLoopingSounds.TryGetValue(sound, out var existingSource) && existingSource != null)
            {
                if (sound.interruptLoopOnReplay)
                {
                    StopAndReturn(existingSource);
                }
                else
                {
                    existingSource.volume = sound.volume;
                    existingSource.pitch = Random.Range(sound.pitchMin, sound.pitchMax);
                    existingSource.outputAudioMixerGroup = sound.mixerGroup;
                    return;
                }
            }

            var audioSource = _audioSourcePool.GetAudioSource();
            audioSource.DOKill();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.loop = true;
            audioSource.pitch = Random.Range(sound.pitchMin, sound.pitchMax);
            audioSource.outputAudioMixerGroup = sound.mixerGroup;

            if (sound.fadeIn)
                FadeIn(audioSource, sound.volume, sound.fadeInTime);
            else
                audioSource.Play();

            _activeLoopingSounds[sound] = audioSource;
        }

        private void PlayOneShotSound(SoundEffectSO sound)
        {
            var audioSource = _audioSourcePool.GetAudioSource();
            audioSource.DOKill();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.loop = false;
            audioSource.pitch = Random.Range(sound.pitchMin, sound.pitchMax);
            audioSource.outputAudioMixerGroup = sound.mixerGroup;
            audioSource.Play();

            var actualDuration = sound.clip.length / Mathf.Max(0.01f, audioSource.pitch);
            StartCoroutine(ReturnAudioSourceAfterClip(audioSource, actualDuration));
        }

        private IEnumerator ReturnAudioSourceAfterClip(AudioSource audioSource, float clipLength)
        {
            yield return new WaitForSeconds(clipLength);

            if (audioSource != null && _audioSourcePool != null)
                _audioSourcePool.ReturnAudioSource(audioSource);
        }

        public void StopLoopingSound(SoundEffectSO sound)
        {
            if (_activeLoopingSounds.TryGetValue(sound, out var audioSource))
            {
                if (audioSource != null)
                {
                    if (sound.fadeOut)
                        FadeOutAndReturnToPool(audioSource, sound.fadeOutTime);
                    else
                        StopAndReturn(audioSource);
                }

                _activeLoopingSounds.Remove(sound);
            }
        }

        public void StopAllLoopingSounds()
        {
            foreach (var audioSource in _activeLoopingSounds.Values)
            {
                if (audioSource != null)
                    StopAndReturn(audioSource);
            }

            _activeLoopingSounds.Clear();
        }

        private void StopAndReturn(AudioSource source)
        {
            source.Stop();
            source.DOKill();
            _audioSourcePool.ReturnAudioSource(source);
        }

        private void FadeIn(AudioSource audioSource, float targetVolume, float duration)
        {
            audioSource.volume = 0;
            audioSource.Play();
            audioSource.DOKill();
#if UNITY_EDITOR
            Debug.Log($"[AudioManager] Fading in to volume {targetVolume} over {duration} seconds");
#endif
            audioSource.DOFade(targetVolume, duration);
        }

        private void FadeOutAndReturnToPool(AudioSource audioSource, float duration)
        {
            audioSource.DOKill();
            audioSource.DOFade(0, duration).OnComplete(() =>
            {
                audioSource.Stop();
                _audioSourcePool.ReturnAudioSource(audioSource);
            });
        }
    }
}
