using System;
using UnityEngine;

namespace Inventory
{
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }

        private Guid _equippedItemGuid = Guid.Empty;

        public event Action<ItemData> OnItemEquipped;
        public event Action OnItemUnequipped;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void EquipItem(ItemData itemData)
        {
            _equippedItemGuid = itemData.Guid;
            OnItemEquipped?.Invoke(itemData);
#if UNITY_EDITOR
            Debug.Log($"[EquipmentManager] Equipped item: {itemData.itemName}");
#endif
        }

        public void UnequipItem()
        {
            _equippedItemGuid = Guid.Empty;
            OnItemUnequipped?.Invoke();
#if UNITY_EDITOR
            Debug.Log("[EquipmentManager] Item unequipped");
#endif
        }

        public bool IsItemEquipped(Guid itemGuid)
        {
            return _equippedItemGuid == itemGuid;
        }

        public Guid GetEquippedItemGuid() => _equippedItemGuid;

        public bool HasEquippedItem() => _equippedItemGuid != Guid.Empty;
    }
}
