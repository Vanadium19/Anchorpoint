using System;
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

        /// <summary>
        /// Crafting speed multiplier of the open workbench.
        /// </summary>
        float CurrentCraftSpeed { get; }

        event Action StateChanged;

        void OpenWorkbench(IExternalUI workbench);
        void CloseWorkbench();
        void StartBatch(RecipeConfig recipe, int count);
        void CancelBatch(int batchId);
        void ClaimReady(int readyItemId);
        int GetMaxCraftable(RecipeConfig recipe);

        List<WorkbenchQueueMemento> GetQueuesSaveData();
        void SetQueuesSaveData(List<WorkbenchQueueMemento> queues, TimeSpan elapsed, Func<string, RecipeConfig> recipeResolver);
    }
}