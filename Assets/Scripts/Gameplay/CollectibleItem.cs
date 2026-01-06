using UnityEngine;
using DG.Tweening;
using Inventory;

namespace Gameplay
{
    public class CollectibleItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;

        public void Interact()
        {
            if (InventoryService.Instance == null || itemData == null)
            {
                Debug.LogWarning("[CollectibleItem] InventoryService or ItemData is null!");
                return;
            }

            var success = InventoryService.Instance.AddItem(itemData.Guid, amount);

            if (success)
            {
                transform.DOScale(0f, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
                Debug.Log("[CollectibleItem] Inventory is full!");
            }
        }

        public string GetInteractPrompt()
        {
            return itemData != null ? $"Pick Up {itemData.itemName}" : "Pick Up";
        }
    }
}
