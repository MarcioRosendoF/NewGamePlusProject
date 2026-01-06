using UnityEngine;
using System;

namespace Inventory
{
    public enum ItemType
    {
        Equippable,
        Consumable
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        private string guidString;
        public string itemName;
        public Sprite icon;
        [TextArea] public string description;
        public ItemType type;

        private Guid? _cachedGuid;
        public Guid Guid
        {
            get
            {
                if (!_cachedGuid.HasValue)
                {
                    _cachedGuid = string.IsNullOrEmpty(guidString)
                        ? Guid.NewGuid()
                        : Guid.Parse(guidString);
                }
                return _cachedGuid.Value;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(guidString))
            {
                guidString = Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
