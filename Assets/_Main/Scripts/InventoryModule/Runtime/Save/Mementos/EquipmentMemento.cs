using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class EquipmentMemento
    {
        public List<EquipmentSlotMemento> Slots = new();
    }
}
