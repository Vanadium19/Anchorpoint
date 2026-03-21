using System;
using System.Collections.Generic;
using InventoryModule;
using UnityEngine;

namespace BuildingModule
{
    [Serializable]
    public class Price
    {
        [SerializeField] private ItemToCount[] value;

        public IEnumerable<ItemToCount> Values => value;
    }

    [Serializable]
    public class ItemToCount
    {
        public ItemDataSo ItemData;
        public int Count;
    }
}
