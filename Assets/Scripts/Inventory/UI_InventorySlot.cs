using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Inventory
{
    public class UI_InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private Transform itemContainer;

        private ItemData _itemData;
        private int _amount;
        private int _slotIndex;
        private UI_InventoryItem _currentItem;
        private InventoryView _inventoryView;

        public UI_InventoryItem CurrentItem => _currentItem;
        public Transform ItemContainer => itemContainer != null ? itemContainer : transform;
        public InventoryView InventoryView => _inventoryView;

        private void Awake()
        {
            _inventoryView = GetComponentInParent<InventoryView>();

            if (itemContainer == null)
                itemContainer = transform;
        }

        public void SetData(ItemData itemData, int amount, int index, GameObject itemPrefab)
        {
            _itemData = itemData;
            _amount = amount;
            _slotIndex = index;

            if (_itemData != null)
            {
                if (_currentItem == null)
                {
                    CreateItem(itemPrefab);
                }
                else
                {
                    _currentItem.Initialize(_itemData, _amount, this);
                }
            }
            else
            {
                ClearItem();
            }
        }

        private void CreateItem(GameObject itemPrefab)
        {
            var itemObj = Instantiate(itemPrefab, itemContainer);
            _currentItem = itemObj.GetComponent<UI_InventoryItem>();
            _currentItem.Initialize(_itemData, _amount, this);
        }

        private void ClearItem()
        {
            if (_currentItem != null)
            {
                _currentItem.PlayDisappearAnimation();
                _currentItem = null;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
        }

        public void NotifyItemDropped(UI_InventorySlot targetSlot)
        {
            if (_inventoryView != null)
            {
                _inventoryView.SwapItems(_slotIndex, targetSlot._slotIndex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_itemData != null && TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.Show(_itemData.itemName, _itemData.description);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.Hide();
            }
        }
    }
}
