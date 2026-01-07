using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(AudioManager))]
    public class AudioSourcePool : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSourcePrefab;
        [SerializeField] private int poolSize = 30;

        private Queue<AudioSource> availableAudioSources = new Queue<AudioSource>();

        private void Start()
        {
            for (var i = 0; i < poolSize; i++)
            {
                var audioSource = Instantiate(audioSourcePrefab, transform);
                audioSource.gameObject.SetActive(false);
                availableAudioSources.Enqueue(audioSource);
            }
        }

        public AudioSource GetAudioSource()
        {
            if (availableAudioSources.Count > 0)
            {
                var audioSource = availableAudioSources.Dequeue();
                audioSource.gameObject.SetActive(true);
                return audioSource;
            }
            else
            {
                var audioSource = Instantiate(audioSourcePrefab, transform);
                return audioSource;
            }
        }

        public void ReturnAudioSource(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(false);
            availableAudioSources.Enqueue(audioSource);
        }
    }
}
