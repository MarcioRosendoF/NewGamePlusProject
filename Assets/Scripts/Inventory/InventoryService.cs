using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using DG.Tweening;
using Gameplay;

namespace Inventory
{
    public class InventoryService : MonoBehaviour
    {
        public static InventoryService Instance { get; private set; }

        [SerializeField] private ItemDatabase itemDatabase;
        [Header("Audio")]
        [SerializeField] private SoundEffectSO genericUseSound;
        [SerializeField] private GameObject worldItemPrefab;
        [SerializeField] private float dropDistance = 0.5f;

        private const int MAX_SLOTS = 9;
        private const float CONSUMABLE_COOLDOWN = 0.5f;
        private const float EQUIP_TOGGLE_COOLDOWN = 0.3f;
        private const float EQUIP_USE_COOLDOWN = 1f;

        private List<InventorySlot> _slots = new List<InventorySlot>();
        private float _lastConsumableUseTime = -999f;
        private float _lastEquipToggleTime = -999f;
        private float _lastEquipUseTime = -999f;

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
                var itemGuid = _slots[index].itemGuid;
                _slots[index].amount -= amount;

                if (_slots[index].amount <= 0)
                {
                    if (IsItemEquipped(itemGuid))
                    {
                        EquipmentManager.Instance?.UnequipItem();
                    }

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
                if (Time.time - _lastConsumableUseTime < CONSUMABLE_COOLDOWN)
                    return false;

                _lastConsumableUseTime = Time.time;

                ExecuteItemBehavior(itemData);
                PlayItemUseSound(itemData);
                OnItemUsed?.Invoke(itemData);

#if UNITY_EDITOR
                Debug.Log($"[InventoryService] Used consumable: {itemData.itemName}");
#endif
                RemoveItem(index, 1);
            }
            else if (itemData.type == ItemType.Equippable)
            {
                if (Time.time - _lastEquipToggleTime < EQUIP_TOGGLE_COOLDOWN)
                    return false;

                _lastEquipToggleTime = Time.time;

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

            if (Time.time - _lastEquipUseTime < EQUIP_USE_COOLDOWN)
                return false;

            _lastEquipUseTime = Time.time;

            ExecuteItemBehavior(itemData);
            PlayItemUseSound(itemData);
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

        private void PlayItemUseSound(ItemData itemData)
        {
            if (AudioManager.Instance == null) return;

            var soundToPlay = itemData.useSound != null ? itemData.useSound : genericUseSound;
            if (soundToPlay != null)
                AudioManager.Instance.PlaySound(soundToPlay);
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

        public void DropItem(int index)
        {
            if (index < 0 || index >= _slots.Count || _slots[index].IsEmpty)
                return;

            var itemData = GetItemAt(index);
            var amount = _slots[index].amount;

            if (itemData == null || worldItemPrefab == null)
                return;

            if (TryGetPlayerAndDirection(out var player, out var facingDir))
            {
                CalculateDropPositions(player.transform.position, facingDir, out var spawnPos, out var targetPos);
                SpawnAndAnimateItem(itemData, amount, spawnPos, targetPos);
            }

            RemoveItem(index, amount);
        }

        private bool TryGetPlayerAndDirection(out GameObject player, out Vector2 facingDir)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            facingDir = Vector2.down;

            if (player == null) return false;

            var movement = player.GetComponent<MovementController>();
            if (movement != null)
            {
                facingDir = movement.LastMoveInput;
            }
            return true;
        }

        private void CalculateDropPositions(Vector3 playerPos, Vector2 facingDir, out Vector3 spawnPos, out Vector3 targetPos)
        {
            spawnPos = (Vector2)playerPos + facingDir * 0.5f;
            targetPos = (Vector2)spawnPos + facingDir * dropDistance;
        }

        private void SpawnAndAnimateItem(ItemData itemData, int amount, Vector3 spawnPos, Vector3 targetPos)
        {
            var itemObj = Instantiate(worldItemPrefab, spawnPos, Quaternion.identity);
            var collectible = itemObj.GetComponent<CollectibleItem>();

            if (collectible != null)
            {
                collectible.Initialize(itemData, amount);
                collectible.AnimateDrop(targetPos);
            }
        }
    }
}
