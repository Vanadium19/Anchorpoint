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

        private readonly Dictionary<string, WorkbenchCraftQueue> _queues = new();
        private IExternalUI _currentWorkbench;

        private WorkbenchCraftQueue CurrentQueue
        {
            get
            {
                var key = GetQueueKey(_currentWorkbench);
                return key != null && _queues.TryGetValue(key, out var q) ? q : null;
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

        public List<WorkbenchQueueMemento> GetQueuesSaveData()
        {
            var list = new List<WorkbenchQueueMemento>();

            foreach (var kvp in _queues)
            {
                var queue = kvp.Value;
                var data = new WorkbenchQueueMemento
                {
                    InstanceId = kvp.Key,
                    NextBatchId = queue.NextBatchId,
                    NextReadyId = queue.NextReadyId
                };

                foreach (var batch in queue.Batches)
                {
                    data.Batches.Add(new BatchMemento
                    {
                        BatchId = batch.BatchId,
                        RecipeName = batch.Recipe.name,
                        TotalCount = batch.TotalCount,
                        CompletedCount = batch.CompletedCount,
                        CurrentCraftTime = batch.CurrentCraftTime
                    });
                }

                foreach (var ready in queue.ReadyItems)
                {
                    data.ReadyItems.Add(new ReadyMemento
                    {
                        ReadyId = ready.ReadyId,
                        SourceBatchId = ready.SourceBatchId,
                        RecipeName = ready.Recipe.name,
                        Count = ready.Count
                    });
                }

                list.Add(data);
            }

            return list;
        }

        public void SetQueuesSaveData(List<WorkbenchQueueMemento> queues, TimeSpan elapsed, Func<string, RecipeConfig> recipeResolver)
        {
            _queues.Clear();

            foreach (var q in queues)
            {
                var queue = new WorkbenchCraftQueue
                {
                    NextBatchId = q.NextBatchId,
                    NextReadyId = q.NextReadyId
                };

                foreach (var b in q.Batches)
                {
                    var recipe = recipeResolver(b.RecipeName);

                    if (recipe == null)
                        continue;

                    queue.Batches.Add(new CraftBatch
                    {
                        BatchId = b.BatchId,
                        Recipe = recipe,
                        TotalCount = b.TotalCount,
                        CompletedCount = b.CompletedCount,
                        CurrentCraftTime = (float)b.CurrentCraftTime
                    });
                }

                foreach (var r in q.ReadyItems)
                {
                    var recipe = recipeResolver(r.RecipeName);

                    if (recipe == null)
                        continue;

                    queue.ReadyItems.Add(new ReadyCraft
                    {
                        ReadyId = r.ReadyId,
                        SourceBatchId = r.SourceBatchId,
                        Recipe = recipe,
                        Count = r.Count
                    });
                }

                _queues[q.InstanceId] = queue;
            }

            if (elapsed.TotalSeconds > 0f)
            {
                var elapsedSec = (float)elapsed.TotalSeconds;
                var changed = false;

                foreach (var queue in _queues.Values)
                {
                    while (queue.Batches.Count > 0)
                    {
                        var batch = queue.Batches[0];
                        var remaining = elapsedSec;

                        while (remaining > 0f && batch.CompletedCount < batch.TotalCount)
                        {
                            var timeToComplete = batch.Recipe.CraftTime - batch.CurrentCraftTime;

                            if (remaining >= timeToComplete)
                            {
                                remaining -= timeToComplete;
                                batch.CompletedCount++;
                                batch.CurrentCraftTime = 0f;

                                var existing = queue.ReadyItems
                                    .FirstOrDefault(r => r.Recipe == batch.Recipe);

                                if (existing != null)
                                {
                                    existing.Count++;
                                }
                                else
                                {
                                    queue.ReadyItems.Add(new ReadyCraft
                                    {
                                        ReadyId = queue.NextReadyId++,
                                        SourceBatchId = batch.BatchId,
                                        Recipe = batch.Recipe,
                                        Count = 1
                                    });
                                }

                                if (batch.CompletedCount >= batch.TotalCount)
                                {
                                    queue.Batches.RemoveAt(0);
                                    batch = queue.Batches.Count > 0 ? queue.Batches[0] : null;

                                    if (batch == null)
                                        break;
                                }

                                changed = true;
                            }
                            else
                            {
                                batch.CurrentCraftTime += remaining;
                                remaining = 0f;
                            }
                        }

                        elapsedSec = remaining;

                        if (queue.Batches.Count == 0 || elapsedSec <= 0f)
                            break;
                    }
                }

                if (changed)
                    StateChanged?.Invoke();
            }
        }

        protected override bool CanHandle(IExternalUI ui) => ui is IRecipeProvider;

        protected override GameObject CreateView(IExternalUI ui)
        {
            _currentWorkbench = ui;
            GetOrCreateQueue(ui);

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
            GetOrCreateQueue(ui);

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

        private string GetQueueKey(IExternalUI ui)
        {
            return ui is IHasInstanceId hasId ? hasId.InstanceId : null;
        }

        private WorkbenchCraftQueue GetOrCreateQueue(IExternalUI ui)
        {
            var key = GetQueueKey(ui);

            if (key == null)
                return null;

            if (!_queues.TryGetValue(key, out var queue))
            {
                queue = new WorkbenchCraftQueue();
                _queues[key] = queue;
            }

            return queue;
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
}
