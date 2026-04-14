using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class InventoryMemento
    {
        public List<ItemMemento> Items = new();
        public int GridWidth;
        public int GridHeight;
    }
}
