using System;
using UnityEngine;
using UtilsModule;

namespace BaseModule
{
    [Serializable]
    public class StorageSizeEffect : BuildingUpgradeEffect
    {
        private const string DescriptionKey = "building_upgrade_storage_format";

        [SerializeField] private Vector2Int sizeIncrease = new(0, 4);

        public override string GetDescription(GameObject building)
        {
            if (!TryGetTarget(building, out var target))
                return string.Empty;

            var size = target.StorageSize;
            var nextSize = size + sizeIncrease;
            return LocalizedText.GetFormatted(DescriptionKey, size.x, size.y, nextSize.x, nextSize.y);
        }

        public override void Apply(GameObject building)
        {
            if (TryGetTarget(building, out var target))
                target.ExpandStorage(sizeIncrease);
        }

        private bool TryGetTarget(GameObject building, out IStorageUpgradeTarget target)
        {
            target = null;
            return sizeIncrease.x >= 0
                && sizeIncrease.y >= 0
                && sizeIncrease != Vector2Int.zero
                && building.TryGetComponent(out target);
        }
    }
}
