using System.Collections.Generic;
using InventoryModule;

namespace BaseModule
{
    public interface IRecipeProvider
    {
        IReadOnlyList<RecipeConfig> Recipes { get; }
    }
}