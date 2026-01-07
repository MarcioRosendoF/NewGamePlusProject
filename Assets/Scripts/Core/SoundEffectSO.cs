using UnityEngine;
using UnityEngine.Audio;

namespace Core
{
    [CreateAssetMenu(fileName = "New Sound Effect", menuName = "Audio/Sound Effect")]
    public class SoundEffectSO : ScriptableObject
    {
        [Header("Audio Clip")]
        public AudioClip clip;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Header("Pitch Randomization")]
        [Range(0.5f, 1.5f)]
        public float pitchMin = 0.95f;
        [Range(0.5f, 1.5f)]
        public float pitchMax = 1.05f;

        [Header("Loop Settings")]
        public bool loop = false;
        [Tooltip("If true, replaying a looping sound will restart it")]
        public bool interruptLoopOnReplay = false;

        [Header("Fade Settings")]
        public bool fadeIn = false;
        [Range(0.1f, 5f)]
        public float fadeInTime = 0.5f;

        public bool fadeOut = false;
        [Range(0.1f, 5f)]
        public float fadeOutTime = 0.5f;

        [Header("Audio Mixer")]
        public AudioMixerGroup mixerGroup;
    }
}
