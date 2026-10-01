using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingUpgradeConfig", menuName = "Game/Configs/Constructing/BuildingUpgradeConfig")]
    public class BuildingUpgradeConfig : ScriptableObject
    {
        public const int BaseLevel = 1;

        [FormerlySerializedAs("levels")]
        [SerializeField] private List<BuildingUpgradeLevel> upgrades = new();

        /// <summary>
        /// Highest reachable level: the base building plus one level per configured upgrade.
        /// </summary>
        public int MaxLevel => BaseLevel + upgrades.Count;

        /// <summary>
        /// Returns the upgrade that brings a building to the given level (2 and above).
        /// </summary>
        public bool TryGetUpgrade(int level, out BuildingUpgradeLevel upgrade)
        {
            var index = level - BaseLevel - 1;
            upgrade = index >= 0 && index < upgrades.Count ? upgrades[index] : null;
            return upgrade != null;
        }
    }
}
