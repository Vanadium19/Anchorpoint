using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class DeathLootStorageMemento
    {
        public List<DeathLootPileMemento> Piles = new();
    }
}
