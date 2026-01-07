using UnityEngine;
using Core;
using DG.Tweening;

namespace Inventory
{
    public class InventoryView : MonoBehaviour, IView
    {
        [Header("References")]
        [SerializeField] private InventoryService inventoryService;
        [SerializeField] private UI_InventorySlot[] inventorySlots;
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject itemPrefab;

        [Header("Animation Settings")]
        [SerializeField] private Vector2 hiddenPosition = new Vector2(1920, 0);
        [SerializeField] private Vector2 visiblePosition = new Vector2(1400, 0);
        [SerializeField] private float animationDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private SoundEffectSO openSound;
        [SerializeField] private SoundEffectSO closeSound;

        public bool IsVisible { get; private set; }
        public bool IsAnimating => _isAnimating;
        public RectTransform PanelTransform => panelTransform;
        private bool _isAnimating;
        private Tween _currentTween;

        private void Awake()
        {
            if (panelTransform == null)
                panelTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterView("Inventory", this);
            }

            panelTransform.anchoredPosition = hiddenPosition;
            IsVisible = false;

            inventoryService.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (inventoryService != null)
            {
                inventoryService.OnInventoryChanged -= RefreshUI;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UnregisterView("Inventory");
            }

            _currentTween?.Kill();
        }

        private void RefreshUI()
        {
            var slots = inventoryService.GetAllSlots();

            for (var i = 0; i < inventorySlots.Length && i < slots.Count; i++)
            {
                var itemData = inventoryService.GetItemAt(i);
                var amount = inventoryService.GetAmountAt(i);
                inventorySlots[i].SetData(itemData, amount, i, itemPrefab);
            }
        }

        public void SwapItems(int fromIndex, int toIndex)
        {
            inventoryService.SwapItems(fromIndex, toIndex);
        }

        public void Show()
        {
            if (_isAnimating || IsVisible) return;

            _currentTween?.Kill();
            _isAnimating = true;

            gameObject.SetActive(true);
            IsVisible = true;

            if (openSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(openSound);

            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;

            _currentTween = panelTransform.DOAnchorPos(visiblePosition, animationDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    _isAnimating = false;
                    if (canvasGroup != null)
                        canvasGroup.blocksRaycasts = true;
                });
        }

        public void Hide()
        {
            if (_isAnimating || !IsVisible) return;

            CancelAllDrags();

            if (TooltipSystem.Instance != null)
                TooltipSystem.Instance.Hide();

            if (closeSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(closeSound);

            _currentTween?.Kill();
            _isAnimating = true;
            IsVisible = false;

            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;

            _currentTween = panelTransform.DOAnchorPos(hiddenPosition, animationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _isAnimating = false;
                    if (canvasGroup != null)
                        canvasGroup.blocksRaycasts = true;
                });
        }

        private void CancelAllDrags()
        {
            foreach (var slot in inventorySlots)
            {
                slot.CurrentItem?.ForceCompleteAnimations();
            }
        }

        public void Toggle()
        {
            if (_isAnimating) return;

            if (IsVisible)
                Hide();
            else
                Show();
        }
    }
}
