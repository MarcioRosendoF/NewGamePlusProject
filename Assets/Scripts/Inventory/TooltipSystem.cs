using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Inventory
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI contentText;

        [SerializeField] private float fadeDuration = 0.15f;

        private Tween _fadeTween;

        private void Awake()
        {
            Instance = this;

            if (canvasGroup == null)
                canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            tooltipPanel.SetActive(false);
        }

        public void Show(string title, string content)
        {
            Show(title, null, content);
        }

        public void Show(string title, string subtitle, string content)
        {
            titleText.text = title;

            if (subtitleText != null)
            {
                if (!string.IsNullOrEmpty(subtitle))
                {
                    subtitleText.text = subtitle;
                    subtitleText.gameObject.SetActive(true);
                }
                else
                {
                    subtitleText.gameObject.SetActive(false);
                }
            }

            contentText.text = content;

            tooltipPanel.SetActive(true);
            _fadeTween?.Kill();

            _fadeTween = canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
        }

        public void Hide()
        {
            _fadeTween?.Kill();
            _fadeTween = canvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => tooltipPanel.SetActive(false));
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
        }
    }
}
