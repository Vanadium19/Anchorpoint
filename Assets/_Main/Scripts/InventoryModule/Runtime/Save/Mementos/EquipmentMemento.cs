using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class EquipmentMemento
    {
        public List<EquipmentSlotMemento> Slots = new();
    }

    [Serializable]
    public class EquipmentSlotMemento
    {
        public string SlotType;
        public ItemMemento Item;
    }
}
