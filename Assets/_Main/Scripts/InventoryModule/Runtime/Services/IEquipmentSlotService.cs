using System.Collections.Generic;

namespace InventoryModule
{
    public interface IEquipmentSlotService
    {
        void RegisterSlot(EquipmentSlot slot);
        void UnregisterSlot(EquipmentSlot slot);
        IReadOnlyList<EquipmentSlot> GetAllSlots();
        EquipmentSlot GetSlotForItem(ItemTable item);
        EquipmentSlot GetSlot(EquipmentSlotType slotType);

        bool CanEquip(EquipmentSlot slot, ItemTable item);
        bool TryEquip(EquipmentSlot slot, ItemTable item);
        void Unequip(EquipmentSlot slot);
        ItemTable ExtractItem(EquipmentSlot slot, out EquipmentSlot extractedFromSlot);
        void RestoreToSlot(EquipmentSlot slot, ItemTable item, InventoryItem existingUI);
        void RestoreSavedItem(EquipmentSlot slot);
        void SaveEquippedItem(EquipmentSlot slot);
    }
}