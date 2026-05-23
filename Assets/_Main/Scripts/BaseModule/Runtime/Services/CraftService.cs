using System;
using System.Collections.Generic;
using System.Linq;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace BaseModule
{
    public class CraftService : ExternalUIHandler, ICraftService, ITickable
    {
        private readonly IInventoryManager _inventoryManager;

        private readonly Dictionary<IExternalUI, WorkbenchCraftQueue> _queues = new();
        private IExternalUI _currentWorkbench;

        private WorkbenchCraftQueue CurrentQueue
        {
            get
            {
                if (_currentWorkbench != null && _queues.TryGetValue(_currentWorkbench, out var q))
                    return q;
                return null;
            }
        }

        public bool IsWorkbenchOpen { get; private set; }
        public RecipeConfig SelectedRecipe { get; private set; }
        public IReadOnlyList<CraftBatch> ActiveBatches => CurrentQueue?.Batches;
        public IReadOnlyList<ReadyCraft> ReadyItems => CurrentQueue?.ReadyItems;

        public event Action StateChanged;

        public CraftService(
            IInventoryManager inventoryManager,
            ExternalUIManager manager,
            DiContainer diContainer,
            Canvas canvas)
            : base(manager, diContainer, canvas)
        {
            _inventoryManager = inventoryManager;
        }

        public void SelectRecipe(RecipeConfig recipe)
        {
            SelectedRecipe = recipe;
            StateChanged?.Invoke();
        }

        public void OpenWorkbench(IExternalUI workbench) { }
        public void CloseWorkbench() { }
        public void StartBatch(RecipeConfig recipe, int count)
        {
            if (recipe == null || count <= 0 || CurrentQueue == null)
                return;

            var maxCraftable = GetMaxCraftable(recipe);

            if (maxCraftable <= 0)
                return;

            var actualCount = Mathf.Min(count, maxCraftable);

            ConsumeIngredients(recipe, actualCount);

            CurrentQueue.Batches.Add(new CraftBatch
            {
                BatchId = CurrentQueue.NextBatchId++,
                Recipe = recipe,
                TotalCount = actualCount,
                CompletedCount = 0,
                CurrentCraftTime = 0f
            });

            StateChanged?.Invoke();
        }

        public void CancelBatch(int batchId)
        {
            var queue = CurrentQueue;

            if (queue == null)
                return;

            var batch = queue.Batches.FirstOrDefault(b => b.BatchId == batchId);

            if (batch == null)
                return;

            RefundIngredients(batch);

            queue.Batches.Remove(batch);
            StateChanged?.Invoke();
        }

        public void ClaimReady(int readyItemId)
        {
            var queue = CurrentQueue;

            if (queue == null)
                return;

            var ready = queue.ReadyItems.FirstOrDefault(r => r.ReadyId == readyItemId);

            if (ready == null)
                return;

            foreach (var result in ready.Recipe.Results)
            {
                if (!_inventoryManager.AddItemToInventory(result.Item, result.Count))
                    return;
            }

            ready.Count--;

            if (ready.Count <= 0)
                queue.ReadyItems.Remove(ready);

            StateChanged?.Invoke();
        }

        public int GetMaxCraftable(RecipeConfig recipe)
        {
            var max = int.MaxValue;

            foreach (var ingredient in recipe.Ingredients)
            {
                var have = _inventoryManager.GetItemCount(ingredient.Item);
                var needed = have / ingredient.Count;

                if (needed < max)
                    max = needed;
            }

            return max;
        }

        protected override bool CanHandle(IExternalUI ui) => ui is IRecipeProvider;

        protected override GameObject CreateView(IExternalUI ui)
        {
            _currentWorkbench = ui;

            if (!_queues.ContainsKey(ui))
                _queues[ui] = new WorkbenchCraftQueue();

            var instance = InstantiateView(ui);
            var view = instance.GetComponent<WorkbenchView>();

            if (view != null)
            {
                var recipes = GetRecipes(ui);
                view.Initialize(this, _inventoryManager, recipes);
            }

            IsWorkbenchOpen = true;
            StateChanged?.Invoke();

            return instance;
        }

        protected override void OnReactivated(IExternalUI ui, GameObject view)
        {
            _currentWorkbench = ui;

            if (!_queues.ContainsKey(ui))
                _queues[ui] = new WorkbenchCraftQueue();

            IsWorkbenchOpen = true;
            var workbenchView = view.GetComponent<WorkbenchView>();

            if (workbenchView != null)
                workbenchView.RefreshFromService();

            StateChanged?.Invoke();
        }

        public override void CloseAll()
        {
            base.CloseAll();
            IsWorkbenchOpen = false;
        }

        public void Tick()
        {
            var changed = false;

            foreach (var queue in _queues.Values)
            {
                if (queue.Batches.Count == 0)
                    continue;

                var activeBatch = queue.Batches[0];

                activeBatch.CurrentCraftTime += Time.deltaTime;

                if (activeBatch.CurrentCraftTime >= activeBatch.Recipe.CraftTime)
                {
                    var excess = activeBatch.CurrentCraftTime - activeBatch.Recipe.CraftTime;
                    activeBatch.CompletedCount++;
                    activeBatch.CurrentCraftTime = excess;

                    var existing = queue.ReadyItems
                        .FirstOrDefault(r => r.Recipe == activeBatch.Recipe);

                    if (existing != null)
                    {
                        existing.Count++;
                    }
                    else
                    {
                        queue.ReadyItems.Add(new ReadyCraft
                        {
                            ReadyId = queue.NextReadyId++,
                            SourceBatchId = activeBatch.BatchId,
                            Recipe = activeBatch.Recipe,
                            Count = 1
                        });
                    }

                    if (activeBatch.CompletedCount >= activeBatch.TotalCount)
                    {
                        queue.Batches.RemoveAt(0);
                    }

                    changed = true;
                }
            }

            if (changed)
                StateChanged?.Invoke();
        }

        private void ConsumeIngredients(RecipeConfig recipe, int count)
        {
            foreach (var ingredient in recipe.Ingredients)
                _inventoryManager.TryRemoveItems(ingredient.Item, ingredient.Count * count);
        }

        private void RefundIngredients(CraftBatch batch)
        {
            var remaining = batch.TotalCount - batch.CompletedCount;

            foreach (var ingredient in batch.Recipe.Ingredients)
            {
                for (int i = 0; i < remaining; i++)
                    _inventoryManager.AddItemToInventory(ingredient.Item, ingredient.Count);
            }
        }

        private static IReadOnlyList<RecipeConfig> GetRecipes(IExternalUI externalUI)
        {
            if (externalUI is IRecipeProvider provider)
                return provider.Recipes;

            return Array.Empty<RecipeConfig>();
        }
    }

    public class WorkbenchCraftQueue
    {
        public List<CraftBatch> Batches { get; } = new();
        public List<ReadyCraft> ReadyItems { get; } = new();
        public int NextBatchId;
        public int NextReadyId;
    }

    public class CraftBatch
    {
        public int BatchId { get; set; }
        public RecipeConfig Recipe { get; set; }
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public float CurrentCraftTime { get; set; }
        public float Progress => Recipe != null && Recipe.CraftTime > 0f
            ? Mathf.Clamp01(CurrentCraftTime / Recipe.CraftTime)
            : 0f;
        public int RemainingToCraft => TotalCount - CompletedCount;
    }

    public class ReadyCraft
    {
        public int ReadyId { get; set; }
        public int SourceBatchId { get; set; }
        public RecipeConfig Recipe { get; set; }
        public int Count { get; set; } = 1;
    }
}