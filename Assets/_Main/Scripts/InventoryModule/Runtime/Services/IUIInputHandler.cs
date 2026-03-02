namespace InventoryModule
{
    public interface IUIInputHandler
    {
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
