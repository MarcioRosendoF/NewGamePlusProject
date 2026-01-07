using UnityEngine;

namespace Core
{
    public class MusicManager : MonoBehaviour
    {
        [Header("Background Music")]
        [SerializeField] private SoundEffectSO backgroundMusic;

        private void Start()
        {
            PlayBackgroundMusic();
        }

        private void PlayBackgroundMusic()
        {
            if (backgroundMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(backgroundMusic);
#if UNITY_EDITOR
                Debug.Log($"[MusicManager] Playing background music: {backgroundMusic.name}");
#endif
            }
        }

        public void StopMusic()
        {
            if (backgroundMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopLoopingSound(backgroundMusic);
            }
        }

        private void OnDestroy()
        {
            StopMusic();
        }
    }
}
