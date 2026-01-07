using UnityEngine;
using DG.Tweening;
using Core;

namespace Gameplay
{
    public class InteractiveDecor : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string promptMessage = "Interact";
        [SerializeField] private SoundEffectSO interactSound;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Animation Settings")]
        [SerializeField] private float squashDuration = 0.12f;
        [SerializeField] private float returnDuration = 0.18f;
        [SerializeField] private float squashScaleMultiplier = 0.7f;
        [SerializeField] private float squashOffsetYMultiplier = 0.15f;
        [SerializeField] private float cooldownTime = 0.5f;

        private bool isAnimating;
        private float lastInteractTime;
        private Vector3 originalPosition;
        private Vector3 originalScale;

        private Tween _activeTween;
        private Sequence _activeSequence;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            originalPosition = spriteRenderer.transform.localPosition;
            originalScale = spriteRenderer.transform.localScale;
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        private void KillAllTweens()
        {
            _activeTween?.Kill();
            _activeSequence?.Kill();

            if (spriteRenderer != null)
            {
                spriteRenderer.DOKill();
                spriteRenderer.transform.DOKill();
            }
        }

        private void ResetVisuals()
        {
            KillAllTweens();

            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = originalPosition;
                spriteRenderer.transform.localScale = originalScale;

                var color = spriteRenderer.color;
                if (color.a < 1f)
                {
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }

        public void Interact()
        {
            if (isAnimating || Time.time < lastInteractTime + cooldownTime) return;

            lastInteractTime = Time.time;

            if (interactSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(interactSound);

            PlaySquashAnimation();
        }

        public string GetInteractPrompt()
        {
            return promptMessage;
        }

        private void PlaySquashAnimation()
        {
            ResetVisuals();
            isAnimating = true;

            var offsetY = spriteRenderer.bounds.size.y * squashOffsetYMultiplier;

            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(spriteRenderer.transform.DOLocalMoveY(originalPosition.y - offsetY, squashDuration).SetEase(Ease.InQuad));
            _activeSequence.Join(spriteRenderer.transform.DOScaleY(originalScale.y * squashScaleMultiplier, squashDuration).SetEase(Ease.InQuad));
            _activeSequence.Append(spriteRenderer.transform.DOLocalMoveY(originalPosition.y, returnDuration).SetEase(Ease.OutBack));
            _activeSequence.Join(spriteRenderer.transform.DOScaleY(originalScale.y, returnDuration).SetEase(Ease.OutBack));
            _activeSequence.OnComplete(() => isAnimating = false);
        }
    }
}
