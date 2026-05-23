using System.Collections.Generic;
using InventoryModule;
using UnityEngine;

namespace BaseModule
{
    public class Workbench : MonoBehaviour, IExternalUI, IRecipeProvider
    {
        [SerializeField] private string displayName = "Workbench";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private List<RecipeConfig> recipes = new();

        public string DisplayName => displayName;
        public GameObject UIPrefab => uiPrefab;
        public IReadOnlyList<RecipeConfig> Recipes => recipes;
    }
}