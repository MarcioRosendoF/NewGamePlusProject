using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryService : MonoBehaviour
    {
        public static InventoryService Instance { get; private set; }

        [SerializeField] private ItemDatabase itemDatabase;
        private const int MAX_SLOTS = 9;

        private List<InventorySlot> _slots = new List<InventorySlot>();

        public event Action OnInventoryChanged;
        public event Action<ItemData> OnItemUsed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            if (itemDatabase == null)
            {
                Debug.LogError("[InventoryService] ItemDatabase reference is missing! Assign it in the Inspector.");
                return;
            }

            itemDatabase.Initialize();

            for (var i = 0; i < MAX_SLOTS; i++)
            {
                _slots.Add(new InventorySlot(Guid.Empty, 0));
            }
        }

        public bool AddItem(Guid itemGuid, int amount = 1)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].itemGuid == itemGuid)
                {
                    _slots[i].amount += amount;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }

            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _slots[i].itemGuid = itemGuid;
                    _slots[i].amount = amount;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(int index, int amount = 1)
        {
            if (index >= 0 && index < _slots.Count && !_slots[index].IsEmpty)
            {
                _slots[index].amount -= amount;
                if (_slots[index].amount <= 0)
                {
                    _slots[index].itemGuid = Guid.Empty;
                    _slots[index].amount = 0;
                }
                OnInventoryChanged?.Invoke();
            }
        }

        public void SwapItems(int indexA, int indexB)
        {
            if (indexA >= 0 && indexA < _slots.Count && indexB >= 0 && indexB < _slots.Count)
            {
                var temp = _slots[indexA];
                _slots[indexA] = _slots[indexB];
                _slots[indexB] = temp;
                OnInventoryChanged?.Invoke();
            }
        }

        public ItemData GetItemAt(int index)
        {
            if (index >= 0 && index < _slots.Count && !_slots[index].IsEmpty)
            {
                return itemDatabase.GetItemByGuid(_slots[index].itemGuid);
            }
            return null;
        }

        public int GetAmountAt(int index)
        {
            if (index >= 0 && index < _slots.Count)
            {
                return _slots[index].amount;
            }
            return 0;
        }

        public List<InventorySlot> GetAllSlots() => _slots;

        public bool UseItem(int index)
        {
            if (index < 0 || index >= _slots.Count || _slots[index].IsEmpty)
                return false;

            var itemData = GetItemAt(index);
            if (itemData == null)
                return false;

            if (itemData.type == ItemType.Consumable)
            {
                ExecuteItemBehavior(itemData);
                OnItemUsed?.Invoke(itemData);

#if UNITY_EDITOR
                Debug.Log($"[InventoryService] Used consumable: {itemData.itemName}");
#endif
                RemoveItem(index, 1);
            }
            else if (itemData.type == ItemType.Equippable)
            {
                if (EquipmentManager.Instance != null)
                {
                    if (EquipmentManager.Instance.IsItemEquipped(itemData.Guid))
                    {
                        EquipmentManager.Instance.UnequipItem();
                    }
                    else
                    {
                        EquipmentManager.Instance.EquipItem(itemData);
                    }
                }
            }

            return true;
        }

        public bool IsItemEquipped(Guid itemGuid)
        {
            return EquipmentManager.Instance != null && EquipmentManager.Instance.IsItemEquipped(itemGuid);
        }

        public bool UseEquippedItemWithoutToggle()
        {
            if (EquipmentManager.Instance == null || !EquipmentManager.Instance.HasEquippedItem())
                return false;

            var equippedGuid = EquipmentManager.Instance.GetEquippedItemGuid();
            var itemData = itemDatabase.GetItemByGuid(equippedGuid);
            if (itemData == null)
                return false;

            ExecuteItemBehavior(itemData);
            OnItemUsed?.Invoke(itemData);

#if UNITY_EDITOR
            Debug.Log($"[InventoryService] Used equipped item: {itemData.itemName}");
#endif
            return true;
        }

        private void ExecuteItemBehavior(ItemData itemData)
        {
            if (itemData.behaviorPrefab != null)
            {
                var behavior = itemData.behaviorPrefab.GetComponent<ItemBehavior>();
                if (behavior != null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        behavior.OnUse(player);
                    }
                }
            }
        }

        public void LoadFromSlots(List<InventorySlot> slots)
        {
            _slots.Clear();
            _slots.AddRange(slots);

            while (_slots.Count < MAX_SLOTS)
            {
                _slots.Add(new InventorySlot(Guid.Empty, 0));
            }

            OnInventoryChanged?.Invoke();
        }
    }
}
