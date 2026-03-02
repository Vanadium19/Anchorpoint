using System.Collections.Generic;

namespace InventoryModule
{
    public sealed class EquipmentSlotService : IEquipmentSlotService
    {
        private readonly List<EquipmentSlot> _slots = new List<EquipmentSlot>();

        public void RegisterSlot(EquipmentSlot slot)
        {
            if (slot != null && !_slots.Contains(slot))
                _slots.Add(slot);
        }

        public void UnregisterSlot(EquipmentSlot slot)
        {
            if (slot != null)
                _slots.Remove(slot);
        }

        public IReadOnlyList<EquipmentSlot> GetAllSlots()
        {
            return _slots;
        }

        public EquipmentSlot GetSlotForItem(ItemTable item)
        {
            if (item == null) 
                return null;

            foreach (var slot in _slots)
            {
                if (slot.GetItem() == item)
                    return slot;
            }

            return null;
        }
    }
}
