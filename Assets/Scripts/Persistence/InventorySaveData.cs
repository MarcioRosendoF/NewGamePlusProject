using System;
using System.Collections.Generic;
using Inventory;

namespace Persistence
{
    [Serializable]
    public class InventorySaveData
    {
        public List<SerializableInventorySlot> slots = new List<SerializableInventorySlot>();

        public InventorySaveData(List<InventorySlot> inventorySlots)
        {
            foreach (var slot in inventorySlots)
            {
                slots.Add(new SerializableInventorySlot(slot.itemGuid.ToString(), slot.amount));
            }
        }
    }

    [Serializable]
    public class SerializableInventorySlot
    {
        public string guidString;
        public int amount;

        public SerializableInventorySlot(string guidString, int amount)
        {
            this.guidString = guidString;
            this.amount = amount;
        }

        public InventorySlot ToInventorySlot()
        {
            var guid = string.IsNullOrEmpty(guidString) ? Guid.Empty : Guid.Parse(guidString);
            return new InventorySlot(guid, amount);
        }
    }
}
