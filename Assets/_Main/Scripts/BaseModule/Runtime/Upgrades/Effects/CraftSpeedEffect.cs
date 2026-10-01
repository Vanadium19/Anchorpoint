using System;
using UnityEngine;
using UtilsModule;

namespace BaseModule
{
    [Serializable]
    public class CraftSpeedEffect : BuildingUpgradeEffect
    {
        private const string DescriptionKey = "building_upgrade_craft_speed_format";

        [SerializeField] [Min(0.01f)] private float multiplier = 1.25f;

        public override string GetDescription(GameObject building) => LocalizedText.GetFormatted(DescriptionKey, multiplier.ToString("0.##"));

        public override void Apply(GameObject building)
        {
            if (multiplier > 0f && building.TryGetComponent<ICraftSpeedUpgradeTarget>(out var target))
                target.SetCraftSpeedMultiplier(multiplier);
        }
    }
}
