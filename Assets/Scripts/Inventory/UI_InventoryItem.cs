using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Inventory
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UI_InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private CanvasGroup canvasGroup;

        private ItemData _itemData;
        private int _amount;
        private UI_InventorySlot _originSlot;
        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Vector3 _originalLocalPosition;
        private int _originalSiblingIndex;
        private Canvas _rootCanvas;
        private bool _isDragging;

        private Tween _dragTween;
        private Tween _scaleTween;
        private Tween _rotateTween;
        private Tween _fadeTween;
        private Sequence _returnSequence;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        }

        private void Start()
        {
            if (!_isDragging)
                CaptureHomeState();
        }

        public void Initialize(ItemData itemData, int amount, UI_InventorySlot originSlot)
        {
            _itemData = itemData;
            _amount = amount;
            _originSlot = originSlot;

            KillAllTweens();
            UpdateVisuals();

            if (!_isDragging)
            {
                CaptureHomeState();
                ResetTransformState();
            }

            PlayAppearAnimation();
        }

        private void CaptureHomeState()
        {
            if (_originSlot == null) return;

            _originalParent = _originSlot.ItemContainer;
            _originalLocalPosition = Vector3.zero;
            _originalPosition = _originalParent.position;
            _originalSiblingIndex = 0;
        }

        private void UpdateVisuals()
        {
            if (_itemData != null)
            {
                iconImage.sprite = _itemData.icon;
                iconImage.enabled = true;
                quantityText.text = _amount > 1 ? _amount.ToString() : "";
            }
        }

        private void PlayAppearAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_originSlot != null && _originSlot.InventoryView != null && _originSlot.InventoryView.IsAnimating)
            {
                eventData.pointerDrag = null;
                return;
            }

            CaptureHomeState();

            transform.SetParent(_rootCanvas.transform);
            transform.SetAsLastSibling();

            _isDragging = true;

            PlayDragStartAnimation(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                PlayDragFollowAnimation(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            _dragTween?.Kill();
            _scaleTween?.Kill();
            _rotateTween?.Kill();

            canvasGroup.blocksRaycasts = true;

            var targetSlot = eventData.pointerEnter?.GetComponent<UI_InventorySlot>();

            if (targetSlot != null && targetSlot != _originSlot)
            {
                PlaySwapAnimation();
                _originSlot.NotifyItemDropped(targetSlot);
            }
            else
            {
                PlayReturnAnimation();
            }
        }

        private void PlayDragStartAnimation(Vector3 targetPosition)
        {
            _dragTween?.Kill();
            _scaleTween?.Kill();
            _rotateTween?.Kill();
            _fadeTween?.Kill();

            _fadeTween = canvasGroup.DOFade(0.6f, 0.2f);
            canvasGroup.blocksRaycasts = false;

            _dragTween = transform.DOMove(targetPosition, 0.2f).SetEase(Ease.OutQuad);
            _scaleTween = transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
            _rotateTween = transform.DORotate(new Vector3(0, 0, 5f), 0.2f);
        }

        private void PlayDragFollowAnimation(Vector3 targetPosition)
        {
            _dragTween?.Kill();
            _dragTween = transform.DOMove(targetPosition, 0.15f).SetEase(Ease.OutQuad);
        }

        private void PlaySwapAnimation()
        {
            KillAllTweens();

            _fadeTween = canvasGroup.DOFade(1f, 0.15f);
            _scaleTween = transform.DOScale(1.2f, 0.15f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
            {
                ResetTransformState();
                ReturnToOriginalParent();
                _scaleTween = null;
            });
        }

        private void PlayReturnAnimation()
        {
            _returnSequence?.Kill();

            _returnSequence = DOTween.Sequence();
            _returnSequence.Append(transform.DOMove(_originalPosition, 0.4f).SetEase(Ease.OutElastic, 1.2f, 0.3f));
            _returnSequence.Join(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            _returnSequence.Join(transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
            _returnSequence.Join(canvasGroup.DOFade(1f, 0.3f));
            _returnSequence.OnComplete(() =>
            {
                ReturnToOriginalParent();
                _returnSequence = null;
            });
        }

        private void KillAllTweens()
        {
            _dragTween?.Kill();
            _scaleTween?.Kill();
            _rotateTween?.Kill();
            _fadeTween?.Kill();
            _returnSequence?.Kill();

            _dragTween = null;
            _scaleTween = null;
            _rotateTween = null;
            _fadeTween = null;
            _returnSequence = null;
        }

        public void CancelDrag()
        {
            ForceCompleteAnimations();
        }

        public void ForceCompleteAnimations()
        {
            _isDragging = false;
            KillAllTweens();

            ResetTransformState();
            ReturnToOriginalParent();
        }

        private void ResetTransformState()
        {
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        private void ReturnToOriginalParent()
        {
            // Safety check: if for some reason _originalParent is lost, use the slot's container
            var targetParent = _originalParent != null ? _originalParent : _originSlot.ItemContainer;

            transform.SetParent(targetParent);
            transform.SetSiblingIndex(_originalSiblingIndex);
            transform.localPosition = _originalLocalPosition;
        }

        public void PlayDisappearAnimation(System.Action onComplete = null)
        {
            _isDragging = false;
            KillAllTweens();

            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0, 0.2f));
            sequence.Join(transform.DOScale(0, 0.2f));
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                if (gameObject != null)
                    Destroy(gameObject);
            });
        }
    }
}
