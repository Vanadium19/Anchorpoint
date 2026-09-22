using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseModule
{
    [Serializable]
    public class WorkbenchUpgradeLevel
    {
        [SerializeField] private List<RecipeConfig> recipes = new();

        public IReadOnlyList<RecipeConfig> Recipes => recipes;
    }
}
