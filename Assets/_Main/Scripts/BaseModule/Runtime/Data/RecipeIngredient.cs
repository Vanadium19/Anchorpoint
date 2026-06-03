using System;
using InventoryModule;
using UnityEngine;

namespace BaseModule
{
    [Serializable]
    public class RecipeIngredient
    {
        [SerializeField] private ItemDataSo item;
        [SerializeField] private int count = 1;

        public ItemDataSo Item => item;
        public int Count => count;
    }
}
