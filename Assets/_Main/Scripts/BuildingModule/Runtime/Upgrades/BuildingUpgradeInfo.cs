using System.Collections.Generic;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingUpgradeInfo
    {
        public string BuildingName { get; set; }
        public int CurrentLevel { get; set; }
        public int NextLevel { get; set; }
        public bool CanUpgrade { get; set; }
        public bool CanAfford { get; set; }
        public GameObject NextVisualPrefab { get; set; }
        public IReadOnlyList<PriceItemInfo> PriceItems { get; set; }
    }
}
