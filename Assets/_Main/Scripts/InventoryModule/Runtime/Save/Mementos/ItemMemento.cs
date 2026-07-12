using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class ItemMemento
    {
        public string ItemDataName;
        public int X;
        public int Y;
        public int StackCount;
        public bool IsRotated;
        public int DurabilityCurrent;
        public int MagazineAmmo;
        public int ReserveAmmo;
        public List<ContainerMemento> NestedContainers = new();
    }
}
