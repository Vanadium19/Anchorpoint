using System;

namespace BuildingModule
{
    public class BuildingController : IDisposable
    {
        private readonly BuildingView _view;
        private readonly BuildingConfig _config;
        private readonly BuildingModel _model;
        private readonly IStorageService _storageService;
        private readonly IRepairModifierService _repairModifierService;

        public BuildingController(
            BuildingView view,
            BuildingConfig config,
            IStorageService storageService,
            IRepairModifierService repairModifierService)
        {
            _view = view;
            _config = config;
            _storageService = storageService;
            _repairModifierService = repairModifierService;
            _model = new BuildingModel(config.MaxHealth, config.ConstructionTime);

            _view.Initialize(_model);
            _view.ConfigureRepairInteraction(
                config.DisplayName,
                GetRepairDuration,
                CanRepair,
                Repair);
            _model.StateChanged += OnStateChanged;
            ApplyState(_model.State);
        }

        public BuildingModel Model => _model;
        public bool IsDestroyed => _view == null;

        public void Tick(float deltaTime) => _model.TickConstruction(deltaTime);

        public void Dispose() => _model.StateChanged -= OnStateChanged;

        private float GetRepairDuration() => _repairModifierService.GetRepairDuration(_config.RepairTime);

        private bool CanRepair() =>
            _model.CanRepair &&
            _storageService.CanAfford(_config.RepairPrice, _repairModifierService.CostMultiplier);

        private void Repair()
        {
            if (!_model.CanRepair)
                return;

            if (!_storageService.Spend(_config.RepairPrice, _repairModifierService.CostMultiplier))
                return;

            _model.Repair();
        }

        private void OnStateChanged(BuildingState state) => ApplyState(state);

        private void ApplyState(BuildingState state)
        {
            if (_view == null)
                return;

            _view.SetStateVisual(state, _config.BrokenAlpha);
        }
    }
}
