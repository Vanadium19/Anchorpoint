using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InventoryModule
{
    public class UIInputHandler : IUIInputHandler
    {
        public InventoryItem GetInventoryItemUnderMouse() => GetItemUnderMouse(null);

        public InventoryItem GetInventoryItemUnderMouse(AbstractItem excludeItem) => GetItemUnderMouse(excludeItem);

        public EquipmentSlot GetEquipmentSlotUnderMouse()
        {
            var mousePosition = Input.mousePosition;
            var pointerData = new PointerEventData(EventSystem.current) { position = mousePosition, };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<EquipmentSlot>();

                if (slot != null)
                    return slot;
            }

            return null;
        }

        public InventoryDropZone GetDropZoneUnderMouse() => GetDropZoneUnderMouse(null);

        public InventoryDropZone GetDropZoneUnderMouse(AbstractItem excludeItem)
        {
            var mousePosition = Input.mousePosition;
            var pointerData = new PointerEventData(EventSystem.current) { position = mousePosition, };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (excludeItem != null && (result.gameObject == excludeItem.gameObject || result.gameObject.transform.IsChildOf(excludeItem.transform)))
                    continue;

                var dropZone = result.gameObject.GetComponentInParent<InventoryDropZone>();

                if (dropZone != null)
                    return dropZone;
            }

            return null;
        }

        public AbstractGrid GetGridUnderMouse() => GetGridUnderMouse(null);

        public AbstractGrid GetGridUnderMouse(AbstractItem excludeItem)
        {
            var mousePosition = Input.mousePosition;
            var pointerData = new PointerEventData(EventSystem.current) { position = mousePosition };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (excludeItem != null && (result.gameObject == excludeItem.gameObject || result.gameObject.transform.IsChildOf(excludeItem.transform)))
                    continue;

                var grid = result.gameObject.GetComponentInParent<AbstractGrid>();

                if (grid != null)
                    return grid;
            }

            return null;
        }

        public InventoryItem GetContainerItemUnderMouse() => GetContainerItemUnderMouse(null);

        public InventoryItem GetContainerItemUnderMouse(AbstractItem excludeItem)
        {
            var mousePosition = Input.mousePosition;
            var pointerData = new PointerEventData(EventSystem.current) { position = mousePosition, };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (excludeItem != null && (result.gameObject == excludeItem.gameObject || result.gameObject.transform.IsChildOf(excludeItem.transform)))
                    continue;

                var itemUI = result.gameObject.GetComponentInParent<InventoryItem>();

                if (itemUI != null && itemUI.Item != null && itemUI.Item.IsContainer)
                    return itemUI;
            }

            return null;
        }

        public InventoryItem GetStackTargetUnderMouse(ItemTable currentItem) => GetStackTargetUnderMouse(null, currentItem);

        public InventoryItem GetStackTargetUnderMouse(AbstractItem excludeItem, ItemTable currentItem)
        {
            if (currentItem == null || !currentItem.IsStackable)
                return null;

            var mousePosition = Input.mousePosition;
            var pointerData = new PointerEventData(EventSystem.current) { position = mousePosition, };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (excludeItem != null && (result.gameObject == excludeItem.gameObject || result.gameObject.transform.IsChildOf(excludeItem.transform)))
                    continue;

                var itemUI = result.gameObject.GetComponentInParent<InventoryItem>();

                if (itemUI == null || itemUI == excludeItem || itemUI.Item == null)
                    continue;

                if (itemUI.Item.ItemDataSo != currentItem.ItemDataSo || !itemUI.Item.IsStackable)
                    continue;

                if (itemUI.Item.StackCount < itemUI.Item.MaxStack)
                    return itemUI;
            }

            return null;
        }

        private InventoryItem GetItemUnderMouse(AbstractItem excludeItem)
        {
            var mousePosition = Input.mousePosition;
            var pointerData = new PointerEventData(EventSystem.current) { position = mousePosition, };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (excludeItem != null && (result.gameObject == excludeItem.gameObject || result.gameObject.transform.IsChildOf(excludeItem.transform)))
                    continue;

                var itemUI = result.gameObject.GetComponentInParent<InventoryItem>();

                if (itemUI != null)
                    return itemUI;
            }

            return null;
        }
    }
}