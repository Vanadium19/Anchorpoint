using System;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingFactory
    {
        private readonly BuildingCatalog _catalog;
        private readonly IBuildingRegistry _registry;
        private readonly BuildingControllerFactory _controllerFactory;
        private readonly BuildingLifecycleService _lifecycleService;

        public BuildingFactory(
            BuildingCatalog catalog,
            IBuildingRegistry registry,
            BuildingControllerFactory controllerFactory,
            BuildingLifecycleService lifecycleService)
        {
            _catalog = catalog;
            _registry = registry;
            _controllerFactory = controllerFactory;
            _lifecycleService = lifecycleService;
        }

        public BuildingView Create(string id, Vector3 position, Quaternion rotation)
        {
            if (!_catalog.TryGetConfig(id, out var config))
                return null;

            var view = UnityEngine.Object.Instantiate(config.Prefab, position, rotation);

            view.BuildingConfigId = config.Id;
            view.InstanceId = Guid.NewGuid().ToString();

            if (view.CollisionCollider != null)
                view.CollisionCollider.enabled = false;

            var controller = _controllerFactory.Create(view, config);
            _lifecycleService.Register(controller);
            _registry.RegisterBuilding(view);

            return view;
        }

        public BuildingView CreatePreview(string id, Vector3 position, Quaternion rotation)
        {
            if (!_catalog.TryGetConfig(id, out var config))
                return null;

            var view = UnityEngine.Object.Instantiate(config.Prefab, position, rotation);
            view.BuildingConfigId = config.Id;
            return view;
        }
    }
}
