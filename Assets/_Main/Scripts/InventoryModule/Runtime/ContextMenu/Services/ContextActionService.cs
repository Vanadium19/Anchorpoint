using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InventoryModule.ContextMenu
{
    public class ContextActionService : IContextActionService
    {
        private DiContainer _container;

        private ContainerWindow _windowPrefab;
        private AbstractGrid _gridPrefab;
        private Canvas _canvas;
        private bool _prefabsResolved;

        public ContainerWindow ContainerWindowPrefab
        {
            get
            {
                TryResolvePrefabs();
                return _windowPrefab;
            }
        }

        public AbstractGrid GridPrefab
        {
            get
            {
                TryResolvePrefabs();
                return _gridPrefab;
            }
        }

        public Canvas Canvas
        {
            get
            {
                TryResolvePrefabs();
                return _canvas;
            }
        }

        public bool IsInitialized
        {
            get
            {
                TryResolvePrefabs();
                return _windowPrefab != null && _gridPrefab != null;
            }
        }


        public void SetPrefabs(DiContainer sceneContainer, ContainerWindow windowPrefab, AbstractGrid gridPrefab, Canvas canvas)
        {
            _container = sceneContainer;
            _windowPrefab = windowPrefab;
            _gridPrefab = gridPrefab;
            _canvas = canvas;
            _prefabsResolved = true;
        }

        private void TryResolvePrefabs()
        {
            if (_prefabsResolved)
                return;

            if (_container == null)
                return;

            _windowPrefab = _container.TryResolve<ContainerWindow>();
            _gridPrefab = _container.TryResolve<AbstractGrid>();
            _canvas = _container.TryResolve<Canvas>();
            _prefabsResolved = true;
        }

        public IReadOnlyList<IContextAction> GetActions(ItemTable item)
        {
            if (!IsInitialized)
                return new List<IContextAction>();

            var actions = new List<IContextAction>();

            var preset = item.ItemDataSo.ContextActionPreset;

            if (preset == null)
                return actions;

            var sortedConfigs = preset.GetSortedConfigs();

            foreach (var config in sortedConfigs)
            {
                var action = config.Create(_container, item);

                if (action == null)
                    continue;

                if (!action.IsAvailable)
                    continue;

                actions.Add(action);
            }

            return actions;
        }
    }
}