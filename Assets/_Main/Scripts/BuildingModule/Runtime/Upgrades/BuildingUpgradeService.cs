using System;
using System.Collections.Generic;
using BaseModule;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingUpgradeService : IBuildingUpgradeService, IInitializable, IDisposable
    {
        private readonly IBuildingRegistry _registry;
        private readonly BuildingCatalog _catalog;
        private readonly IInventoryManager _inventoryManager;
        private readonly IBuildingDamageService _damageService;

        private readonly Dictionary<BuildingView, BuildingUpgradeModel> _models = new();

        public event Action<BuildingView> BuildingUpgraded;

        public BuildingUpgradeService(
            IBuildingRegistry registry,
            BuildingCatalog catalog,
            IInventoryManager inventoryManager,
            IBuildingDamageService damageService)
        {
            _registry = registry;
            _catalog = catalog;
            _inventoryManager = inventoryManager;
            _damageService = damageService;
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

            if (!TryGetModel(building, out var model) || !TryGetBuildingConfig(building, out var config))
                return false;

            var nextLevel = model.NextLevel;
            var priceItems = CreatePriceItems(nextLevel?.Price);
            info = new BuildingUpgradeInfo
            {
                BuildingName = config.DisplayName,
                CurrentLevel = model.Level,
                NextLevel = model.Level + 1,
                CanUpgrade = model.CanUpgrade,
                CanAfford = model.CanUpgrade && CanAfford(nextLevel?.Price),
                NextVisualPrefab = nextLevel?.VisualPrefab,
                PriceItems = priceItems
            };

            return true;
        }

        public bool TryUpgrade(BuildingView building)
        {
            if (!TryGetModel(building, out var model) || !model.CanUpgrade)
                return false;

            var nextLevel = model.NextLevel;

            if (!CanAfford(nextLevel?.Price) || !TrySpend(nextLevel?.Price))
                return false;

            model.Upgrade();
            ApplyLevel(building, model);
            BuildingUpgraded?.Invoke(building);
            return true;
        }

        public void RestoreLevel(BuildingView building, int level)
        {
            if (!TryGetModel(building, out var model))
                return;

            model.Restore(level);
            ApplyLevel(building, model);
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

        private bool TryGetBuildingConfig(BuildingView building, out BuildingConfig config)
        {
            config = null;
            return building != null
                && !string.IsNullOrEmpty(building.BuildingConfigId)
                && _catalog.TryGetConfig(building.BuildingConfigId, out config);
        }

        private bool CanAfford(Price price)
        {
            if (price?.Values == null)
                return true;

            foreach (var itemToCount in price.Values)
            {
                if (itemToCount.ItemData != null
                    && _inventoryManager.GetItemCount(itemToCount.ItemData) < itemToCount.Count)
                    return false;
            }

            return true;
        }

        private bool TrySpend(Price price)
        {
            if (price?.Values == null)
                return true;

            foreach (var itemToCount in price.Values)
            {
                if (itemToCount.ItemData != null
                    && !_inventoryManager.TryRemoveItems(itemToCount.ItemData, itemToCount.Count))
                    return false;
            }

            return true;
        }

        private List<PriceItemInfo> CreatePriceItems(Price price)
        {
            var items = new List<PriceItemInfo>();

            if (price?.Values == null)
                return items;

            foreach (var itemToCount in price.Values)
            {
                if (itemToCount.ItemData == null)
                    continue;

                items.Add(new PriceItemInfo
                {
                    ItemData = itemToCount.ItemData,
                    Available = _inventoryManager.GetItemCount(itemToCount.ItemData),
                    Cost = itemToCount.Count
                });
            }

            return items;
        }

        private void ApplyLevel(BuildingView building, BuildingUpgradeModel model)
        {
            building.RenderUpgradeVisual(model.CurrentLevel?.VisualPrefab);

            foreach (var component in building.GetComponents<MonoBehaviour>())
            {
                if (component is IBuildingUpgradeReceiver receiver)
                    receiver.ApplyUpgradeLevel(model.Level);
            }

            if (TryGetBuildingConfig(building, out var config))
            {
                var maxHealth = model.CurrentLevel?.MaxHealth ?? 0f;
                _damageService.SetMaxHealth(building, maxHealth > 0f ? maxHealth : config.MaxHealth);
            }
        }
    }
}
