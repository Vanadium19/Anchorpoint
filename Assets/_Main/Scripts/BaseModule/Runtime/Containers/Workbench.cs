using System;
using System.Collections.Generic;
using InventoryModule;
using UnityEngine;
using UtilsModule;

namespace BaseModule
{
    public class Workbench : MonoBehaviour, IExternalUI, IRecipeProvider, IHasInstanceId, IBuildingUpgradeReceiver
    {
        [SerializeField] private string displayName = "Workbench";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private List<RecipeConfig> recipes = new();
        [SerializeField] private List<WorkbenchUpgradeLevel> upgradeLevels = new();
        [SerializeField] private string _instanceId;

        [Header("Localization")]
        [SerializeField] private string nameKey = "";

        private readonly List<RecipeConfig> _availableRecipes = new();

        private int _currentUpgradeLevel = 1;

        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);
        public GameObject UIPrefab => uiPrefab;
        public IReadOnlyList<RecipeConfig> Recipes => _availableRecipes;
        public string InstanceId { get => _instanceId; set => _instanceId = value; }

        private void Awake()
        {
            _availableRecipes.AddRange(recipes);

            if (string.IsNullOrEmpty(_instanceId))
                _instanceId = Guid.NewGuid().ToString();
        }

        public void ApplyUpgradeLevel(int level)
        {
            var targetLevel = Mathf.Max(1, level);

            if (targetLevel <= _currentUpgradeLevel)
                return;

            for (var currentLevel = _currentUpgradeLevel + 1; currentLevel <= targetLevel; currentLevel++)
            {
                var levelIndex = currentLevel - 2;

                if (levelIndex < 0 || levelIndex >= upgradeLevels.Count)
                    continue;

                var upgradeLevel = upgradeLevels[levelIndex];

                if (upgradeLevel == null)
                    continue;

                foreach (var recipe in upgradeLevel.Recipes)
                {
                    if (recipe != null && !_availableRecipes.Contains(recipe))
                        _availableRecipes.Add(recipe);
                }
            }

            _currentUpgradeLevel = targetLevel;
        }
    }
}
