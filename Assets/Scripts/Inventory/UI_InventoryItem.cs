using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        [SerializeField] private Image equippedIndicator;

        private ItemData _itemData;
        private int _amount;
        private UI_InventorySlot _originSlot;
        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Vector3 _originalLocalPosition;
        private int _originalSiblingIndex;
        private Canvas _rootCanvas;
        private bool _isDragging;
        private bool _isReturning;
        private PointerEventData _activePointerEventData;
        private Vector2 _dropMousePosition;

        private Tween _dragTween;
        private Tween _scaleTween;
        private Tween _rotateTween;
        private Tween _fadeTween;
        private Tween _appearTween;
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

            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnItemEquipped += OnItemEquipped;
                EquipmentManager.Instance.OnItemUnequipped += OnItemUnequipped;
            }

            UpdateEquippedIndicator();
        }

        private void OnDestroy()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnItemEquipped -= OnItemEquipped;
                EquipmentManager.Instance.OnItemUnequipped -= OnItemUnequipped;
            }
        }

        private void OnItemEquipped(ItemData item)
        {
            UpdateEquippedIndicator();
        }

        private void OnItemUnequipped()
        {
            UpdateEquippedIndicator();
        }

        public void Initialize(ItemData itemData, int amount, UI_InventorySlot originSlot, bool playAnimation = true)
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

            if (playAnimation)
            {
                PlayAppearAnimation();
            }

            CheckTooltipAfterCreation();
        }

        private void CheckTooltipAfterCreation()
        {
            if (Mouse.current == null || _originSlot == null)
                return;

            var mousePos = Mouse.current.position.ReadValue();
            _originSlot.OnItemReturnComplete(mousePos);
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

            UpdateEquippedIndicator();
        }

        private void UpdateEquippedIndicator()
        {
            if (equippedIndicator == null) return;

            var isEquipped = _itemData != null &&
                           _itemData.type == ItemType.Equippable &&
                           EquipmentManager.Instance != null &&
                           EquipmentManager.Instance.IsItemEquipped(_itemData.Guid);

            equippedIndicator.enabled = isEquipped;
        }

        private void PlayAppearAnimation()
        {
            _appearTween?.Kill();
            transform.localScale = Vector3.zero;
            _appearTween = transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetAutoKill(false);
            _appearTween.OnKill(() => _appearTween = null);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_originSlot != null && _originSlot.InventoryView != null && _originSlot.InventoryView.IsAnimating)
            {
                eventData.pointerDrag = null;
                return;
            }

            if (_isReturning)
            {
                eventData.pointerDrag = null;
                return;
            }

            CaptureHomeState();

            transform.SetParent(_rootCanvas.transform);
            transform.SetAsLastSibling();

            _isDragging = true;
            _activePointerEventData = eventData;

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
            if (!_isDragging) return;

            _isDragging = false;
            _activePointerEventData = null;

            _dragTween?.Kill();
            _scaleTween?.Kill();
            _rotateTween?.Kill();

            canvasGroup.blocksRaycasts = true;

            var targetSlot = GetSlotUnderCursor(eventData);

            if (targetSlot != null && targetSlot != _originSlot)
            {
                _originSlot.ClearCurrentItemReference();
                _originSlot.NotifyItemDropped(targetSlot);
                KillAllTweens();
                Destroy(gameObject);
            }
            else
            {
                _dropMousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
                PlayReturnAnimation();
            }
        }

        private UI_InventorySlot GetSlotUnderCursor(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<UI_InventorySlot>();
                if (slot != null)
                    return slot;
            }

            return null;
        }

        private void PlayDragStartAnimation(Vector3 targetPosition)
        {
            _dragTween?.Kill();
            _scaleTween?.Kill();
            _rotateTween?.Kill();
            _fadeTween?.Kill();

            _fadeTween = canvasGroup.DOFade(0.6f, 0.2f).SetAutoKill(false);
            _fadeTween.OnKill(() => _fadeTween = null);
            canvasGroup.blocksRaycasts = false;

            _dragTween = transform.DOMove(targetPosition, 0.2f).SetEase(Ease.OutQuad).SetAutoKill(false);
            _dragTween.OnKill(() => _dragTween = null);

            _scaleTween = transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack).SetAutoKill(false);
            _scaleTween.OnKill(() => _scaleTween = null);

            _rotateTween = transform.DORotate(new Vector3(0, 0, 5f), 0.2f).SetAutoKill(false);
            _rotateTween.OnKill(() => _rotateTween = null);
        }

        private void PlayDragFollowAnimation(Vector3 targetPosition)
        {
            _dragTween?.Kill();
            _dragTween = transform.DOMove(targetPosition, 0.08f).SetEase(Ease.OutQuad).SetAutoKill(false);
            _dragTween.OnKill(() => _dragTween = null);
        }


        private void PlayReturnAnimation()
        {
            _returnSequence?.Kill();
            _isReturning = true;

            _returnSequence = DOTween.Sequence();
            _returnSequence.SetAutoKill(false);
            _returnSequence.Append(transform.DOMove(_originalPosition, 0.4f).SetEase(Ease.OutElastic, 0.8f, 0.3f));
            _returnSequence.Join(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            _returnSequence.Join(transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
            _returnSequence.Join(canvasGroup.DOFade(1f, 0.3f));
            _returnSequence.OnComplete(() =>
            {
                _isReturning = false;
                ReturnToOriginalParent();
                _returnSequence = null;
                _originSlot?.OnItemReturnComplete(_dropMousePosition);
            });
            _returnSequence.OnKill(() =>
            {
                _isReturning = false;
                _returnSequence = null;

            });
        }

        private void KillAllTweens()
        {
            _dragTween?.Kill();
            _scaleTween?.Kill();
            _rotateTween?.Kill();
            _fadeTween?.Kill();
            _appearTween?.Kill();
            _returnSequence?.Kill();

            _dragTween = null;
            _scaleTween = null;
            _rotateTween = null;
            _fadeTween = null;
            _appearTween = null;
            _returnSequence = null;
        }

        public void CancelDrag()
        {
            ForceCompleteAnimations();
        }

        public void ForceCompleteAnimations()
        {
            if (_isDragging && _activePointerEventData != null)
            {
                _activePointerEventData.pointerDrag = null;
            }

            _isDragging = false;
            _activePointerEventData = null;

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
