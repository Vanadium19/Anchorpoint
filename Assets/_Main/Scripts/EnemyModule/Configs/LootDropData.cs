using System;
using InventoryModule;
using UnityEngine;

namespace EnemyModule
{
    [Serializable]
    public class LootDropData
    {
        [Min(0f)]
        [SerializeField] private float dropWeight = 1f;
        [SerializeField] private ItemDataSo item;

        public ItemDataSo Item => item;
        public float DropWeight => dropWeight;

        public bool IsValid => Item && Item.IsDropable && Item.WorldPrefab && DropWeight > 0f;
    }
}