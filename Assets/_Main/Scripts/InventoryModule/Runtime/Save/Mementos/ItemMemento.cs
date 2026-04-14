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
        public List<ContainerMemento> NestedContainers = new();
    }
}
