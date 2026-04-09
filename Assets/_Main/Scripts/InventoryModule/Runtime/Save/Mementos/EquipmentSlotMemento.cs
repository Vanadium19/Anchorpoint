using System;

namespace InventoryModule
{
    [Serializable]
    public class EquipmentSlotMemento
    {
        public string SlotType;
        public ItemMemento Item;
    }
}
