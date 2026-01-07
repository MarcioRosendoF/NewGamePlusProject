using UnityEngine;
using UnityEngine.InputSystem;
using Inventory;

namespace Gameplay
{
    public class EquipmentInputHandler : MonoBehaviour
    {
        private void OnUseEquippedItem(InputValue value)
        {
            if (InventoryService.Instance == null) return;

            InventoryService.Instance.UseEquippedItemWithoutToggle();
        }
    }
}
