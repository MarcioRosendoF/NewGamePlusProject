using System;

namespace Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public Guid itemGuid;
        public int amount;

        public InventorySlot(Guid guid, int amount)
        {
            this.itemGuid = guid;
            this.amount = amount;
        }

        public bool IsEmpty => itemGuid == Guid.Empty;
    }
}
