using System;
using InventoryModule;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A resource amount paid to install a weapon modification.
    /// </summary>
    [Serializable]
    public class WeaponModificationCost
    {
        [SerializeField] private ItemDataSo item;
        [Min(1)]
        [SerializeField] private int count = 1;

        public ItemDataSo Item => item;
        public int Count => count;
    }
}
