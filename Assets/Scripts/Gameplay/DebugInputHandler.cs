using UnityEngine;
using UnityEngine.InputSystem;
using Inventory;

namespace Gameplay
{
    public class DebugInputHandler : MonoBehaviour
    {
        [SerializeField] private ItemDatabase itemDatabase;

        public void OnGiveItem(InputValue value)
        {
#if UNITY_EDITOR
            if (value.isPressed && InventoryService.Instance != null && itemDatabase != null)
            {
                var allItems = itemDatabase.GetAllItems();
                if (allItems != null && allItems.Count > 0)
                {
                    foreach (var item in allItems)
                    {
                        var success = InventoryService.Instance.AddItem(item.Guid, 1);

                        if (success)
                        {
                            Debug.Log($"[DEBUG] Added item: {item.itemName}");
                        }
                        else
                        {
                            Debug.LogWarning("[DEBUG] Inventory is full!");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[DEBUG] No items in database!");
                }
            }
#endif
        }
    }
}
