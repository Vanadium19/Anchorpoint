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

    //TODO: Replace with Odin serialized dictionary
    [Serializable]
    public class ItemToCount
    {
        public ItemDefinition ItemDefinition;
        public int Count;
    }
}