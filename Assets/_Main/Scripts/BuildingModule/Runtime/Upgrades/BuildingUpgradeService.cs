using System;
using System.Collections.Generic;
using System.Linq;
using BaseModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingUpgradeService : IBuildingUpgradeService, IInitializable, IDisposable
    {
        private readonly IBuildingRegistry _registry;
        private readonly BuildingCatalog _catalog;
        private readonly IStorageService _storageService;

        private readonly Dictionary<BuildingView, BuildingUpgradeModel> _models = new();

        public event Action<BuildingView> BuildingUpgraded;

        public BuildingUpgradeService(
            IBuildingRegistry registry,
            BuildingCatalog catalog,
            IStorageService storageService)
        {
            _registry = registry;
            _catalog = catalog;
            _storageService = storageService;
        }

        public void Initialize()
        {
            _registry.BuildingRegistered += OnBuildingRegistered;
            _registry.BuildingUnregistered += OnBuildingUnregistered;

            foreach (var building in _registry.Buildings)
                RegisterBuilding(building);
        }

        public void Dispose()
        {
            _registry.BuildingRegistered -= OnBuildingRegistered;
            _registry.BuildingUnregistered -= OnBuildingUnregistered;
        }

        public int GetLevel(BuildingView building)
        {
            return TryGetModel(building, out var model) ? model.Level : 1;
        }

        public bool TryGetInfo(BuildingView building, out BuildingUpgradeInfo info)
        {
            info = null;

            if (!IsActive(building)
                || !TryGetModel(building, out var model)
                || !TryGetBuildingConfig(building, out var config))
                return false;

            var nextLevel = model.NextLevel;
            info = new BuildingUpgradeInfo
            {
                BuildingName = config.DisplayName,
                CurrentLevel = model.Level,
                NextLevel = model.Level + 1,
                CanUpgrade = model.CanUpgrade,
                CanAfford = model.CanUpgrade && _storageService.CanAfford(nextLevel?.Price),
                CurrentMaxHealth = GetMaxHealth(model.CurrentLevel, config),
                NextMaxHealth = nextLevel != null ? GetMaxHealth(nextLevel, config) : 0f,
                NextEffectDescriptions = GetEffectDescriptions(nextLevel, building.gameObject),
                NextVisualPrefab = nextLevel?.VisualPrefab,
                PriceItems = _storageService.GetPriceInfo(config.DisplayName, nextLevel?.Price).Items
            };

            return true;
        }

        public bool TryUpgrade(BuildingView building)
        {
            if (!IsActive(building) || !TryGetModel(building, out var model) || !model.CanUpgrade)
                return false;

            if (!_storageService.Spend(model.NextLevel?.Price))
                return false;

            model.Upgrade();
            ApplyLevel(building, model);
            ApplyEffects(building, model.Level, model.Level);
            BuildingUpgraded?.Invoke(building);
            return true;
        }

        public void RestoreLevel(BuildingView building, int level)
        {
            if (!TryGetModel(building, out var model))
                return;

            var previousLevel = model.Level;
            model.Restore(level);
            ApplyLevel(building, model);
            ApplyEffects(building, previousLevel + 1, model.Level);
        }

        private void OnBuildingRegistered(BuildingView building) => RegisterBuilding(building);

        private void OnBuildingUnregistered(BuildingView building) => _models.Remove(building);

        private void RegisterBuilding(BuildingView building)
        {
            if (building == null || _models.ContainsKey(building))
                return;

            if (!TryGetBuildingConfig(building, out var config) || config.UpgradeConfig == null)
                return;

            var model = new BuildingUpgradeModel(config.UpgradeConfig);
            _models.Add(building, model);
            ApplyLevel(building, model);
        }

        private bool TryGetModel(BuildingView building, out BuildingUpgradeModel model)
        {
            model = null;
            return building != null && _models.TryGetValue(building, out model);
        }

        private static bool IsActive(BuildingView building) =>
            building != null
            && building.TryGet<BuildingModel>(out var buildingModel)
            && buildingModel.State == BuildingState.Active;

        private bool TryGetBuildingConfig(BuildingView building, out BuildingConfig config)
        {
            config = null;
            return building != null
                && !string.IsNullOrEmpty(building.BuildingConfigId)
                && _catalog.TryGetConfig(building.BuildingConfigId, out config);
        }

        private void ApplyLevel(BuildingView building, BuildingUpgradeModel model)
        {
            building.RenderUpgradeVisual(model.CurrentLevel?.VisualPrefab);

            if (TryGetBuildingConfig(building, out var config) && building.TryGet<BuildingModel>(out var buildingModel))
                buildingModel.SetMaxHealth(GetMaxHealth(model.CurrentLevel, config));
        }

        private void ApplyEffects(BuildingView building, int fromLevel, int toLevel)
        {
            if (!TryGetBuildingConfig(building, out var config))
                return;

            for (var level = fromLevel; level <= toLevel; level++)
            {
                if (!config.UpgradeConfig.TryGetUpgrade(level, out var upgradeLevel))
                    continue;

                foreach (var effect in upgradeLevel.Effects)
                    effect?.Apply(building.gameObject);
            }
        }

        private static List<string> GetEffectDescriptions(BuildingUpgradeLevel level, GameObject building)
        {
            return level?.Effects
                .Where(effect => effect != null)
                .Select(effect => effect.GetDescription(building))
                .Where(description => !string.IsNullOrEmpty(description))
                .ToList()
                ?? new List<string>();
        }

        private static float GetMaxHealth(BuildingUpgradeLevel level, BuildingConfig config)
        {
            var maxHealth = level?.MaxHealth ?? 0f;
            return maxHealth > 0f ? maxHealth : config.MaxHealth;
        }
    }
}
