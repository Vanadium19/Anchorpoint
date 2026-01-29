using System;
using InputModule;
using Zenject;

namespace InventoryModule
{
    public class InventoryPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly InventoryModel _model;
        private readonly InventoryView _view;
        private readonly InventoryConfig _config;
        private readonly ItemCatalog _catalog;
        private readonly IInputMap _input;

        public InventoryPresenter(InventoryModel model,InventoryView view,InventoryConfig config,ItemCatalog catalog,IInputMap input)
        {
            _model = model;
            _view = view;
            _config = config;
            _catalog = catalog;
            _input = input;
        }

        public void Initialize()
        {
            _view.Initialize(_config, _catalog);
            _model.Updated += OnInventoryUpdated;
            OnInventoryUpdated();
        }

        public void Dispose()
        {
            _model.Updated -= OnInventoryUpdated;
        }

        public void Tick()
        {
            if (_input.IsInventoryPressed)
            {
                bool newState = !_view.IsVisible;
                _view.Toggle(newState);
            }
        }

        private void OnInventoryUpdated()
        {
            _view.Render(_model.Items);
        }
    }
}