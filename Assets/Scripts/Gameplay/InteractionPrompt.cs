using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Gameplay
{
    public class InteractionPrompt : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float floatDistance = 10f;
        [SerializeField] private float floatDuration = 1.5f;

        private Interactor playerInteractor;
        private Transform targetTransform;
        private Sequence floatSequence;
        private Vector3 baseOffset = new Vector3(0, 1.5f, 0);

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0;
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInteractor = player.GetComponent<Interactor>();
                if (playerInteractor != null)
                {
                    playerInteractor.OnInteractableChanged += OnInteractableChanged;
                }
            }
        }

        private void OnDestroy()
        {
            if (playerInteractor != null)
            {
                playerInteractor.OnInteractableChanged -= OnInteractableChanged;
            }
            
            KillTweens();
        }

        private void Update()
        {
            if (targetTransform != null && canvasGroup.alpha > 0)
            {
                transform.position = targetTransform.position + baseOffset;
            }
        }

        private void OnInteractableChanged(IInteractable interactable)
        {
            if (interactable != null)
            {
                var monoBehaviour = interactable as MonoBehaviour;
                if (monoBehaviour != null)
                {
                    Show(interactable.GetInteractPrompt(), monoBehaviour.transform);
                }
            }
            else
            {
                Hide();
            }
        }

        private void Show(string prompt, Transform target)
        {
            targetTransform = target;
            promptText.text = $"[E] {prompt}";
            transform.position = target.position + baseOffset;

            KillTweens();

            canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
            transform.DOScale(1f, fadeDuration).From(0.8f).SetEase(Ease.OutBack);

            floatSequence = DOTween.Sequence();
            floatSequence.Append(transform.DOMoveY(transform.position.y + floatDistance / 100f, floatDuration).SetEase(Ease.InOutSine));
            floatSequence.Append(transform.DOMoveY(transform.position.y, floatDuration).SetEase(Ease.InOutSine));
            floatSequence.SetLoops(-1);
        }

        private void Hide()
        {
            targetTransform = null;
            KillTweens();

            canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad);
            transform.DOScale(0.8f, fadeDuration).SetEase(Ease.InBack);
        }

        private void KillTweens()
        {
            canvasGroup.DOKill();
            transform.DOKill();
            floatSequence?.Kill();
        }
    }
}
