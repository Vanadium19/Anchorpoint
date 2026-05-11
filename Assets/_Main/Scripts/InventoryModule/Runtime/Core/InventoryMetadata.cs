// TODO: Код взят из ассета
using System;

namespace InventoryModule
{
    [Serializable]
    public abstract class InventoryMetadata
    {
        [NonSerialized]
        public ItemTable ItemTable;

        public virtual void Initialize(ItemTable itemTable) => ItemTable = itemTable;

        public virtual void Awake() { }
    }
}
