using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class ContainerMemento
    {
        public List<ItemMemento> Items = new();
    }
}
