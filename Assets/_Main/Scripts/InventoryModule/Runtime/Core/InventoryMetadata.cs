using System;
using UnityEngine;

namespace InventoryModule
{
    [Serializable]
    public abstract class InventoryMetadata
    {
        public ItemTable ItemTable { get; protected set; }

        public virtual void Initialize(ItemTable itemTable)
        {
            ItemTable = itemTable;
        }

        public virtual void Awake() { }
    }
}
