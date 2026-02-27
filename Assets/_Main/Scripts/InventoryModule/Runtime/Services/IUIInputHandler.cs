using UnityEngine.EventSystems;

namespace InventoryModule
{
    public interface IUIInputHandler
    {
        void HandleClick(PointerEventData eventData, ItemTable item, AbstractItem itemUI);
        void HandleDoubleClick(PointerEventData eventData, ItemTable item);
        void HandleRightClick(PointerEventData eventData, ItemTable item);
        void HandleDragBegin(PointerEventData eventData, ItemTable item, AbstractItem itemUI);
        void HandleDrag(PointerEventData eventData, AbstractItem itemUI);
        void HandleDragEnd(PointerEventData eventData, AbstractItem itemUI);

        InventoryItem GetInventoryItemUnderMouse();
        InventoryItem GetInventoryItemUnderMouse(AbstractItem excludeItem);
        EquipmentSlot GetEquipmentSlotUnderMouse();
        InventoryDropZone GetDropZoneUnderMouse();
        InventoryDropZone GetDropZoneUnderMouse(AbstractItem excludeItem);
        AbstractGrid GetGridUnderMouse();
        AbstractGrid GetGridUnderMouse(AbstractItem excludeItem);
        InventoryItem GetContainerItemUnderMouse();
        InventoryItem GetContainerItemUnderMouse(AbstractItem excludeItem);
        InventoryItem GetStackTargetUnderMouse(ItemTable currentItem);
        InventoryItem GetStackTargetUnderMouse(AbstractItem excludeItem, ItemTable currentItem);
    }
}
