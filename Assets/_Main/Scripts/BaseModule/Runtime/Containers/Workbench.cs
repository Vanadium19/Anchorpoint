using System;
using System.Collections.Generic;
using InventoryModule;
using UnityEngine;
using UtilsModule;

namespace BaseModule
{
    public class Workbench : MonoBehaviour, IExternalUI, IRecipeProvider, IHasInstanceId, ICraftSpeedSource, IRecipeUpgradeTarget, ICraftSpeedUpgradeTarget
    {
        [SerializeField] private string displayName = "Workbench";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private List<RecipeConfig> recipes = new();
        [SerializeField] private string _instanceId;

        [Header("Localization")]
        [SerializeField] private string nameKey = "";

        private readonly List<RecipeConfig> _availableRecipes = new();

        private float _craftSpeedMultiplier = 1f;

        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);
        public GameObject UIPrefab => uiPrefab;
        public IReadOnlyList<RecipeConfig> Recipes => _availableRecipes;
        public string InstanceId { get => _instanceId; set => _instanceId = value; }
        public float CraftSpeedMultiplier => _craftSpeedMultiplier;

        private void Awake()
        {
            _availableRecipes.AddRange(recipes);

            if (string.IsNullOrEmpty(_instanceId))
                _instanceId = Guid.NewGuid().ToString();
        }

        public void UnlockRecipes(IEnumerable<RecipeConfig> recipes)
        {
            foreach (var recipe in recipes)
            {
                if (!_availableRecipes.Contains(recipe))
                    _availableRecipes.Add(recipe);
            }
        }

        public void SetCraftSpeedMultiplier(float multiplier) => _craftSpeedMultiplier = multiplier;
    }
}
