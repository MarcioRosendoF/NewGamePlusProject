using UnityEngine;
using System;
using Core;

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
        [SerializeField] private string guidString;
        public string itemName;
        public Sprite icon;
        [TextArea] public string description;
        public ItemType type;
        [TextArea] public string usageDescription;
        [Header("Visual (Equippables Only)")]
        [Tooltip("Sprite shown overhead when equipped")]
        public Sprite equippedSprite;
        [Tooltip("Sprite shown overhead when using (F key)")]
        public Sprite useSprite;

        [Header("Behavior")]
        public GameObject behaviorPrefab;

        [Header("Audio")]
        [Tooltip("Optional: Override generic use sound for this specific item")]
        public SoundEffectSO useSound;

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

        public string GetItemTypeLabel()
        {
            return type switch
            {
                ItemType.Consumable => "<color=#8B2500>CONSUMABLE</color>",
                ItemType.Equippable => "<color=#1A4D8F>EQUIPPABLE</color>",
                _ => "UNKNOWN"
            };
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
