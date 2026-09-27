using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilsModule;

namespace BaseModule
{
    [Serializable]
    public class RecipeUnlockEffect : BuildingUpgradeEffect
    {
        private const string DescriptionKey = "building_upgrade_recipes_format";

        [SerializeField] private List<RecipeConfig> recipes = new();

        public override string GetDescription(GameObject building) => LocalizedText.GetFormatted(DescriptionKey, recipes.Count(recipe => recipe != null));

        public override void Apply(GameObject building)
        {
            if (building.TryGetComponent<IRecipeUpgradeTarget>(out var target))
                target.UnlockRecipes(recipes.Where(recipe => recipe != null));
        }
    }
}
