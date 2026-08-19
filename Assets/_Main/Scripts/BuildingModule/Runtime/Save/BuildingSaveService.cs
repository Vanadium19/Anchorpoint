using System.Collections.Generic;
using UnityEngine;
using BaseModule;
using InventoryModule;

namespace BuildingModule
{
    public class BuildingSaveService : IBuildingSaveService
    {
        private readonly IBuildingRegistry _registry;
        private readonly BuildingCatalog _catalog;
        private readonly ItemCatalog _itemCatalog;

        public BuildingSaveService(
            IBuildingRegistry registry,
            BuildingCatalog catalog,
            ItemCatalog itemCatalog)
        {
            _registry = registry;
            _catalog = catalog;
            _itemCatalog = itemCatalog;
        }

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
                PositionX = building.transform.position.x,
                PositionY = building.transform.position.y,
                PositionZ = building.transform.position.z,
                RotationY = building.transform.rotation.eulerAngles.y
            };

            var hasId = building as IHasInstanceId;

            if (hasId != null)
                snapshot.InstanceId = hasId.InstanceId;

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
                {
                    containerMemento.Items.Add(ItemSerializer.Serialize(item));
                }

                snapshot.Containers.Add(containerMemento);
            }
        }

        public BuildingView RestoreFromSnapshot(BuildingSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.BuildingId))
                return null;

            if (!_catalog.TryGetConfig(snapshot.BuildingId, out var config))
                return null;

            var position = new Vector3(snapshot.PositionX, snapshot.PositionY, snapshot.PositionZ);
            var rotation = Quaternion.Euler(0, snapshot.RotationY, 0);

            var building = Object.Instantiate(config.Prefab, position, rotation);

            building.BuildingConfigId = config.Id;
            _registry.RegisterBuilding(building);

            var hasId = building as IHasInstanceId;

            if (hasId != null && !string.IsNullOrEmpty(snapshot.InstanceId))
                hasId.InstanceId = snapshot.InstanceId;

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

        public void ClearAll()
        {
            _registry.Clear();
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

            for (int i = 0; i < containers.Count && i < grids.Count; i++)
            {
                var grid = grids[i];
                var containerMemento = containers[i];

                foreach (var itemMemento in containerMemento.Items)
                {
                    ItemSerializer.RestoreItem(grid, itemMemento, _itemCatalog);
                }
            }
        }
    }
}
