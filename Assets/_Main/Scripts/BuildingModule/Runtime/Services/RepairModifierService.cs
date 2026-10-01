using UnityEngine;

namespace BuildingModule
{
    public class RepairModifierService : IRepairModifierService
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly BuildingCatalog _buildingCatalog;

        public RepairModifierService(
            IBuildingRegistry buildingRegistry,
            BuildingCatalog buildingCatalog)
        {
            _buildingRegistry = buildingRegistry;
            _buildingCatalog = buildingCatalog;
        }

        public float CostMultiplier => 1f - RepairReductionPercent / 100f;
        public float RepairReductionPercent => GetRepairReductionPercent();

        public float GetRepairDuration(float baseDuration) =>
            Mathf.Max(baseDuration, 0f) * CostMultiplier;

        private float GetRepairReductionPercent()
        {
            var reductionPercent = 0f;

            foreach (var building in _buildingRegistry.Buildings)
            {
                if (building == null)
                    continue;

                if (!_buildingCatalog.TryGetConfig(building.BuildingConfigId, out var config))
                    continue;

                if (config.RepairReductionPercent <= reductionPercent)
                    continue;

                if (!building.TryGet<BuildingModel>(out var model))
                    continue;

                if (model.State != BuildingState.Active)
                    continue;

                reductionPercent = config.RepairReductionPercent;
            }

            return Mathf.Clamp(reductionPercent, 0f, 100f);
        }
    }
}
