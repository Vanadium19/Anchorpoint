using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingCatalog", menuName = "Game/Configs/Constructing/BuildingCatalog")]
    public class BuildingCatalog : ScriptableObject
    {
        //TODO: Replace with Odin serialized dictionary
        [SerializeField] private List<BuildingConfig> configs;

        private void OnValidate()
        {
            var success = configs.GroupBy(config => config.name)
                .All(group => group.Count() == 1);

            if (success)
                return;

            Debug.LogError($"Config with this {nameof(BuildingName)} exists");
            var last = configs[^1];
            configs.Remove(last);
        }

        public bool TryGetConfig(BuildingName buildingName, out BuildingConfig config)
        {
            config = configs.FirstOrDefault(config => config.Name == buildingName);
            return config;
        }
    }
}