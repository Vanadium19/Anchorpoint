using System;
using System.Collections.Generic;
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

    [Serializable]
    public class RecipeResult
    {
        [SerializeField] private ItemDataSo item;
        [SerializeField] private int count = 1;

        public ItemDataSo Item => item;
        public int Count => count;
    }

    [CreateAssetMenu(fileName = "RecipeConfig", menuName = "Game/Configs/Constructing/RecipeConfig")]
    public class RecipeConfig : ScriptableObject
    {
        [SerializeField] private string recipeName = "Recipe";
        [SerializeField] private List<RecipeIngredient> ingredients = new();
        [SerializeField] private List<RecipeResult> results = new();
        [SerializeField] private float craftTime = 1f;

        public string RecipeName => recipeName;
        public IReadOnlyList<RecipeIngredient> Ingredients => ingredients;
        public IReadOnlyList<RecipeResult> Results => results;
        public float CraftTime => craftTime;
    }
}