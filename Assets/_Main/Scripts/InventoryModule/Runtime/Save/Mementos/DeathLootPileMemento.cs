using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class DeathLootPileMemento
    {
        public string SceneName;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public List<ItemMemento> Items = new();
    }
}
