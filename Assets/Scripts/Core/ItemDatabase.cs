using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemData> items = new List<ItemData>();

        private Dictionary<Guid, ItemData> _itemLookup;

        public void Initialize()
        {
            _itemLookup = new Dictionary<Guid, ItemData>();
            foreach (var item in items)
            {
                if (item != null && item.Guid != Guid.Empty)
                {
                    _itemLookup[item.Guid] = item;
                }
            }
        }

        public ItemData GetItemByGuid(Guid guid)
        {
            if (_itemLookup == null)
                Initialize();

            return _itemLookup.TryGetValue(guid, out var item) ? item : null;
        }

        public List<ItemData> GetAllItems() => items;
    }
}
