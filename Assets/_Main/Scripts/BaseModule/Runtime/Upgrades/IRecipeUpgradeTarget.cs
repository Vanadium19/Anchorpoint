using System.Collections.Generic;

namespace BaseModule
{
    public interface IRecipeUpgradeTarget
    {
        /// <summary>
        /// Makes the given recipes available.
        /// </summary>
        void UnlockRecipes(IEnumerable<RecipeConfig> recipes);
    }
}
