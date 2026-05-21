using System;
using InventoryModule;
using UnityEngine;

namespace SpawnModule
{
    [Serializable]
    public class LootEntry
    {
        [SerializeField] private ItemDataSo item;
        [SerializeField] private int weight = 1;
        [SerializeField] private int minCount = 1;
        [SerializeField] private int maxCount = 1;
        [SerializeField] private bool isStacked = false;

        public ItemDataSo Item => item;
        public int Weight => weight;
        public int MinCount => minCount;
        public int MaxCount => maxCount;
        public bool IsStacked => isStacked;

#if UNITY_EDITOR
        public void SetItem(ItemDataSo newItem) => item = newItem;
        public void SetWeight(int newWeight) => weight = newWeight;
        public void SetMinCount(int newMin) => minCount = Mathf.Max(1, newMin);
        public void SetMaxCount(int newMax) => maxCount = Mathf.Max(minCount, newMax);
        public void SetIsStacked(bool value) => isStacked = value;
#endif
    }
}
