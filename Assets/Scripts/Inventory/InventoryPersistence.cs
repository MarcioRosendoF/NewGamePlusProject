using UnityEngine;
using Persistence;

namespace Inventory
{
    public class InventoryPersistence : MonoBehaviour
    {
        InventoryService inventoryService;

        private const string SaveFileName = "inventory_save.json";
        private const float SAVE_DEBOUNCE_INTERVAL = 0.5f;

        private bool _isDirty = false;
        private float _lastSaveTime = 0f;

        private void Start()
        {
            inventoryService = InventoryService.Instance;
            LoadInventory();

            if (inventoryService != null)
            {
                inventoryService.OnInventoryChanged += MarkDirty;
            }
        }

        private void Update()
        {
            if (_isDirty && Time.time - _lastSaveTime >= SAVE_DEBOUNCE_INTERVAL)
            {
                SaveInventory();
                _isDirty = false;
                _lastSaveTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            if (inventoryService != null)
            {
                inventoryService.OnInventoryChanged -= MarkDirty;
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                SaveInventory();
            }
#endif
        }

        private void OnApplicationQuit()
        {
            SaveInventory();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveInventory();
            }
        }

        private void MarkDirty()
        {
            _isDirty = true;
        }

        public void SaveInventory()
        {
            if (inventoryService == null) return;

            var saveData = new InventorySaveData(inventoryService.GetAllSlots());
            FileHandler.SaveToFile(saveData, SaveFileName);
        }

        public void LoadInventory()
        {
            var saveData = FileHandler.LoadFromFile<InventorySaveData>(SaveFileName);
            if (saveData != null && saveData.slots != null)
            {
                var slots = new System.Collections.Generic.List<InventorySlot>();
                foreach (var serializedSlot in saveData.slots)
                {
                    slots.Add(serializedSlot.ToInventorySlot());
                }
                inventoryService.LoadFromSlots(slots);
            }
        }
    }
}
