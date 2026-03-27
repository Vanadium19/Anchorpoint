using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingCatalog", menuName = "Game/Configs/Constructing/BuildingCatalog")]
    public class BuildingCatalog : ScriptableObject
    {
        [SerializeField] private List<BuildingConfig> configs;
        [SerializeField] private List<BuildingCategoryConfig> categoryConfigs;

        private void OnValidate()
        {
            if (configs == null || configs.Count == 0)
                return;

            var validConfigs = configs.Where(c => !string.IsNullOrEmpty(c.Id)).ToList();
            var duplicates = validConfigs.GroupBy(c => c.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (duplicates.Count > 0)
                Debug.LogError("Duplicate Ids found: " + string.Join(", ", duplicates));
        }

        public bool TryGetConfig(string id, out BuildingConfig config)
        {
            config = configs.FirstOrDefault(config => config.Id == id);
            return config;
        }

        public IReadOnlyList<BuildingConfig> GetAll() => configs;

        public IReadOnlyList<BuildingCategoryConfig> GetCategoryConfigs() => categoryConfigs;

        public bool TryGetCategoryConfig(BuildingCategory category, out BuildingCategoryConfig config)
        {
            config = categoryConfigs?.FirstOrDefault(c => c.Category == category);
            return config != null;
        }
    }
}