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
