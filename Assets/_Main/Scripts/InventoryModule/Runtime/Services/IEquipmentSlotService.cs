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
    }
}

