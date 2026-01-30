using System;
using InputModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class InventoryPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly InventoryModel _model;
        private readonly InventoryView _view;
        private readonly InventoryConfig _config;
        private readonly ItemCatalog _catalog;

        private readonly IInputMap _inputMap;
        private readonly IInputService _inputService;

        public InventoryPresenter(InventoryModel model, InventoryView view,
            InventoryConfig config, ItemCatalog catalog,
            IInputMap inputMap, IInputService inputService)
        {
            _model = model;
            _view = view;
            _config = config;
            _catalog = catalog;
            _inputMap = inputMap;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _view.Initialize(_config, _catalog, _inputMap);
            _model.Updated += OnInventoryUpdated;
            _view.ItemDropped += OnItemDropped;
            OnInventoryUpdated();
        }

        public void Dispose()
        {
            _model.Updated -= OnInventoryUpdated;
            _view.ItemDropped -= OnItemDropped;
        }

        public void Tick()
        {
            if (_inputMap.IsInventoryPressed)
            {
                bool newState = !_view.IsVisible;
                _view.Toggle(newState);
                _inputService.SetUIMode(newState);
            }
        }

        private void OnInventoryUpdated()
        {
            _view.Render(_model.Items);
        }

        private void OnItemDropped(InventoryItemView itemView, Vector2Int newPosition)
        {
            bool success = false;
            var item = itemView.Item;
            Vector2Int mouseGridPos = _view.GetMouseGridPosition();

            if (itemView.IsSplitting)
            {
                success = _model.TrySplitItem(item, newPosition, itemView.LocalIsRotated);
            }
            else
            {
                success = _model.TryMoveItem(item, newPosition, itemView.LocalIsRotated, mouseGridPos);
            }

            if (!success)
            {
                OnInventoryUpdated();
            }
        }
    }
}