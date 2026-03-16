namespace InventoryModule
{
    public interface IUIInputHandler
    {
        EquipmentSlot GetEquipmentSlotUnderMouse();
        InventoryDropZone GetDropZoneUnderMouse(AbstractItem excludeItem);
        AbstractGrid GetGridUnderMouse(AbstractItem excludeItem);
        InventoryItem GetContainerItemUnderMouse(AbstractItem excludeItem);
        InventoryItem GetStackTargetUnderMouse(AbstractItem excludeItem, ItemTable currentItem);
    }
}
