using System;

namespace BuildingModule
{
    public class BuildingController : IDisposable
    {
        private readonly BuildingView _view;
        private readonly BuildingConfig _config;
        private readonly BuildingModel _model;

        public BuildingController(BuildingView view, BuildingConfig config)
        {
            _view = view;
            _config = config;
            _model = new BuildingModel(config.MaxHealth, config.ConstructionTime);

            _view.Initialize(_model);
            _model.StateChanged += OnStateChanged;
            ApplyState(_model.State);
        }

        public BuildingModel Model => _model;
        public bool IsDestroyed => _view == null;

        public void Tick(float deltaTime) => _model.TickConstruction(deltaTime);

        public void Dispose() => _model.StateChanged -= OnStateChanged;

        private void OnStateChanged(BuildingState state) => ApplyState(state);

        private void ApplyState(BuildingState state)
        {
            if (_view == null)
                return;

            _view.SetStateVisual(state, _config.BrokenAlpha);
        }
    }
}
