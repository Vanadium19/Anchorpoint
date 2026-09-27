using System.Collections.Generic;
using BaseModule;
using InventoryModule;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingSaveService : IBuildingSaveService
    {
        private readonly IBuildingRegistry _registry;
        private readonly BuildingCatalog _catalog;
        private readonly ItemCatalog _itemCatalog;
        private readonly BuildingFactory _buildingFactory;

        public BuildingSaveService(
            IBuildingRegistry registry,
            BuildingCatalog catalog,
            ItemCatalog itemCatalog,
            BuildingFactory buildingFactory)
        {
            _registry = registry;
            _catalog = catalog;
            _itemCatalog = itemCatalog;
            _buildingFactory = buildingFactory;
        }

        public BuildingView RestoreFromSnapshot(BuildingSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.BuildingId))
                return null;

            if (!_catalog.TryGetConfig(snapshot.BuildingId, out var config))
                return null;

            var position = new Vector3(snapshot.PositionX, snapshot.PositionY, snapshot.PositionZ);
            var rotation = Quaternion.Euler(0f, snapshot.RotationY, 0f);
            var building = _buildingFactory.Create(config.Id, position, rotation);

            if (building == null)
                return null;

            if (!string.IsNullOrEmpty(snapshot.InstanceId))
                building.InstanceId = snapshot.InstanceId;

            if (building.TryGet<BuildingModel>(out var model))
            {
                if (snapshot.HasRuntimeState)
                {
                    model.Restore(
                        snapshot.State,
                        snapshot.CurrentHealth,
                        snapshot.ConstructionRemainingTime);
                }
                else
                {
                    model.Restore(BuildingState.Active, model.MaxHealth, 0f);
                }
            }

            var gridView = building.GetComponent<IInventoryGridView>();

            if (gridView != null && snapshot.Containers.Count > 0)
                RestoreContainerItems(gridView, snapshot.Containers);

            return building;
        }

        public IReadOnlyList<BuildingSnapshot> GetAllSnapshots()
        {
            var snapshots = new List<BuildingSnapshot>();

            foreach (var building in _registry.Buildings)
            {
                var snapshot = CreateSnapshot(building);

                if (snapshot != null)
                    snapshots.Add(snapshot);
            }

            return snapshots;
        }

        public void ClearAll() => _registry.Clear();

        private BuildingSnapshot CreateSnapshot(BuildingView building)
        {
            if (building == null)
                return null;

            var config = GetBuildingConfig(building);

            if (config == null)
                return null;

            var snapshot = new BuildingSnapshot
            {
                BuildingId = config.Id,
                InstanceId = building.InstanceId,
                PositionX = building.transform.position.x,
                PositionY = building.transform.position.y,
                PositionZ = building.transform.position.z,
                RotationY = building.transform.rotation.eulerAngles.y
            };

            if (building.TryGet<BuildingModel>(out var model))
            {
                snapshot.HasRuntimeState = true;
                snapshot.State = model.State;
                snapshot.CurrentHealth = model.CurrentHealth;
                snapshot.ConstructionRemainingTime = model.ConstructionRemainingTime;
            }

            SerializeContainers(building, snapshot);

            return snapshot;
        }

        private void SerializeContainers(BuildingView building, BuildingSnapshot snapshot)
        {
            var gridView = building.GetComponent<IInventoryGridView>();

            if (gridView == null)
                return;

            foreach (var grid in gridView.Grids)
            {
                var containerMemento = new ContainerMemento();

                foreach (var item in grid.GetAllItems())
                    containerMemento.Items.Add(ItemSerializer.Serialize(item));

                snapshot.Containers.Add(containerMemento);
            }
        }

        private BuildingConfig GetBuildingConfig(BuildingView building)
        {
            if (building == null || string.IsNullOrEmpty(building.BuildingConfigId))
                return null;

            _catalog.TryGetConfig(building.BuildingConfigId, out var config);

            return config;
        }

        private void RestoreContainerItems(IInventoryGridView container, List<ContainerMemento> containers)
        {
            var grids = container.Grids;

            for (var i = 0; i < containers.Count && i < grids.Count; i++)
            {
                var grid = grids[i];
                var containerMemento = containers[i];

                foreach (var itemMemento in containerMemento.Items)
                    ItemSerializer.RestoreItem(grid, itemMemento, _itemCatalog);
            }
        }
    }
}
