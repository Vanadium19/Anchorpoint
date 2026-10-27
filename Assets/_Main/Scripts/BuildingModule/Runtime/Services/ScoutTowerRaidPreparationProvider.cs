using ComponentsModule;

namespace BuildingModule
{
    public sealed class ScoutTowerRaidPreparationProvider : IRaidPreparationProvider
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly BuildingCatalog _buildingCatalog;

        public ScoutTowerRaidPreparationProvider(
            IBuildingRegistry buildingRegistry,
            BuildingCatalog buildingCatalog)
        {
            _buildingRegistry = buildingRegistry;
            _buildingCatalog = buildingCatalog;
        }

        public bool TryGetPreparation(out RaidPreparationInfo preparationInfo)
        {
            preparationInfo = default;

            var hasPreparation = false;

            foreach (var building in _buildingRegistry.Buildings)
            {
                if (!TryGetActiveScoutTowerConfig(building, out var config))
                    continue;

                if (config.RaidPreparationSeconds <= 0f)
                    continue;

                if (hasPreparation &&
                    config.RaidPreparationSeconds <= preparationInfo.PreparationSeconds)
                    continue;

                preparationInfo = new RaidPreparationInfo(
                    config.RaidPreparationSeconds,
                    config.RaidWarningLocalizationKey,
                    config.RaidWarningDisplaySeconds);

                hasPreparation = true;
            }

            return hasPreparation;
        }

        private bool TryGetActiveScoutTowerConfig(
            BuildingView building,
            out BuildingConfig config)
        {
            config = null;

            if (building == null)
                return false;

            if (!_buildingCatalog.TryGetConfig(building.BuildingConfigId, out config))
                return false;

            if (!config.IsScoutTower)
                return false;

            if (!building.TryGet<BuildingModel>(out var model))
                return false;

            return model.State == BuildingState.Active;
        }
    }
}
