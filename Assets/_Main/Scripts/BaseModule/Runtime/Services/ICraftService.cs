using System.Collections.Generic;
using InventoryModule;

namespace BaseModule
{
    public interface ICraftService
    {
        bool IsWorkbenchOpen { get; }
        RecipeConfig SelectedRecipe { get; }
        void SelectRecipe(RecipeConfig recipe);
        IReadOnlyList<CraftBatch> ActiveBatches { get; }
        IReadOnlyList<ReadyCraft> ReadyItems { get; }

        event System.Action StateChanged;

        void OpenWorkbench(IExternalUI workbench);
        void CloseWorkbench();
        void StartBatch(RecipeConfig recipe, int count);
        void CancelBatch(int batchId);
        void ClaimReady(int readyItemId);
        int GetMaxCraftable(RecipeConfig recipe);
    }
}