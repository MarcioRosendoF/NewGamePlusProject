using UnityEngine;
using DG.Tweening;
using Inventory;
using Core;

namespace Gameplay
{
    public class CollectibleItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;
        [SerializeField] private SoundEffectSO pickupSound;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private Tween _dropTween;

        private void OnDestroy()
        {
            _dropTween?.Kill();
        }

        public void Initialize(ItemData data, int count)
        {
            itemData = data;
            amount = count;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer != null && itemData != null)
            {
                _spriteRenderer.sprite = itemData.icon;
            }
        }

        private bool _isCollected;

        public void AnimateDrop(Vector3 targetPos)
        {
            _dropTween?.Kill();
            _dropTween = transform.DOJump(targetPos, 0.3f, 1, 0.4f)
                .SetEase(Ease.OutQuad)
                .OnKill(() => _dropTween = null);
        }

        public void Interact()
        {
            if (_isCollected) return;

            if (InventoryService.Instance == null || itemData == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[CollectibleItem] InventoryService or ItemData is null!");
#endif
                return;
            }

            var success = InventoryService.Instance.AddItem(itemData.Guid, amount);

            if (success)
            {
                _isCollected = true;
                _dropTween?.Kill();

                if (pickupSound != null && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(pickupSound);

                transform.DOScale(0f, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("[CollectibleItem] Inventory is full!");
#endif
            }
        }

        public string GetInteractPrompt()
        {
            return itemData != null ? $"Pick Up {itemData.itemName}" : "Pick Up";
        }
    }
}
