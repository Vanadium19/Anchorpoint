using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SaveModule;
using BaseModule;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CampSaveModule
{
    public class WorkbenchSaveable : ISaveable
    {
        private const string Key = "workbench_queues";

        private readonly ICraftService _craftService;

        public string SaveKey => Key;

        public WorkbenchSaveable(ICraftService craftService)
        {
            _craftService = craftService;
        }

        public string CreateMemento()
        {
            var memento = new WorkbenchQueuesMemento
            {
                SavedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Queues = _craftService.GetQueuesSaveData()
            };

            return JsonConvert.SerializeObject(memento);
        }

        public void RestoreMemento(string data)
        {
            var memento = JsonConvert.DeserializeObject<WorkbenchQueuesMemento>(data);

            if (memento == null || memento.Queues.Count == 0)
                return;

            var elapsed = TimeSpan.FromMilliseconds(
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - memento.SavedAtUnixMs);

            var recipeResolver = BuildRecipeResolver();

            _craftService.SetQueuesSaveData(memento.Queues, elapsed, recipeResolver);
        }

        private static Func<string, RecipeConfig> BuildRecipeResolver()
        {
            var map = new Dictionary<string, RecipeConfig>();

            var providers = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);

            foreach (var mb in providers)
            {
                if (mb is IRecipeProvider provider)
                {
                    foreach (var recipe in provider.Recipes)
                    {
                        map[recipe.name] = recipe;

                        if (!map.ContainsKey(recipe.RecipeName))
                            map[recipe.RecipeName] = recipe;
                    }
                }
            }

            return name => map.TryGetValue(name, out var r) ? r : null;
        }
    }
}
