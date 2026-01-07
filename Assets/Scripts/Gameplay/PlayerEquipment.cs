using UnityEngine;
using Inventory;
using System.Collections;

namespace Gameplay
{
    public class PlayerEquipment : MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private SpriteRenderer overheadSpriteRenderer;

        [Header("Flash Settings")]
        [SerializeField] private float flashDuration = 0.5f;

        private ItemData _equippedWeapon;
        private Coroutine _flashCoroutine;

        private void Start()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnItemEquipped += OnItemEquipped;
                EquipmentManager.Instance.OnItemUnequipped += OnItemUnequipped;
            }

            if (InventoryService.Instance != null)
            {
                InventoryService.Instance.OnItemUsed += OnItemUsed;
            }

            if (overheadSpriteRenderer != null)
            {
                overheadSpriteRenderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnItemEquipped -= OnItemEquipped;
                EquipmentManager.Instance.OnItemUnequipped -= OnItemUnequipped;
            }

            if (InventoryService.Instance != null)
            {
                InventoryService.Instance.OnItemUsed -= OnItemUsed;
            }
        }

        private void OnItemUsed(ItemData item)
        {
            if (item.type == ItemType.Equippable &&
                EquipmentManager.Instance != null &&
                EquipmentManager.Instance.IsItemEquipped(item.Guid) &&
                item.useSprite != null)
            {
                FlashUseSprite(item);
            }
        }

        private void OnItemEquipped(ItemData item)
        {
            EquipItem(item);
        }

        private void OnItemUnequipped()
        {
            UnequipWeapon();
        }

        private void EquipItem(ItemData item)
        {
            _equippedWeapon = item;

            if (overheadSpriteRenderer != null)
            {
                var spriteToShow = item.equippedSprite != null ? item.equippedSprite : item.icon;
                overheadSpriteRenderer.sprite = spriteToShow;
                overheadSpriteRenderer.enabled = true;
#if UNITY_EDITOR
                Debug.Log($"[PlayerEquipment] Equipped overhead: {item.itemName}");
#endif
            }
        }

        private void FlashUseSprite(ItemData item)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashUseSpriteCoroutine(item));
        }

        private IEnumerator FlashUseSpriteCoroutine(ItemData item)
        {
            if (overheadSpriteRenderer != null && item.useSprite != null)
            {
                overheadSpriteRenderer.sprite = item.useSprite;
                overheadSpriteRenderer.enabled = true;

                yield return new WaitForSeconds(flashDuration);

                var equippedSprite = item.equippedSprite != null ? item.equippedSprite : item.icon;
                overheadSpriteRenderer.sprite = equippedSprite;
            }
        }

        public void UnequipWeapon()
        {
            _equippedWeapon = null;
            if (overheadSpriteRenderer != null)
            {
                overheadSpriteRenderer.enabled = false;
#if UNITY_EDITOR
                Debug.Log("[PlayerEquipment] Weapon unequipped");
#endif
            }
        }

        public ItemData GetEquippedWeapon() => _equippedWeapon;
        public bool HasWeaponEquipped() => _equippedWeapon != null;
    }
}
