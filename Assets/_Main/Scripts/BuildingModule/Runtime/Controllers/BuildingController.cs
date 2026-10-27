using System;
using ComponentsModule;
using Zenject;

namespace BuildingModule
{
    public class BuildingController : IDisposable
    {
        private readonly BuildingView _view;
        private readonly BuildingConfig _config;
        private readonly BuildingModel _model;
        private readonly IStorageService _storageService;
        private readonly IRepairModifierService _repairModifierService;
        private readonly DefenseBuildingController _defenseController;
        private readonly MinefieldController _minefieldController;

        public BuildingController(
            BuildingView view,
            BuildingConfig config,
            IStorageService storageService,
            IRepairModifierService repairModifierService,
            IBuildingRegistry buildingRegistry,
            [InjectOptional] ICombatTargetSource combatTargetSource)
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

            if (config.IsDefensive)
            {
                _defenseController = new DefenseBuildingController(
                    view,
                    _model,
                    config,
                    combatTargetSource);
            }

            if (config.IsMinefield && view.MinefieldView != null)
            {
                var minefieldModel = new MinefieldModel(config.MineCount);

                _minefieldController = new MinefieldController(
                    view,
                    _model,
                    minefieldModel,
                    view.MinefieldView,
                    config,
                    buildingRegistry);
            }

            _model.StateChanged += OnStateChanged;
            ApplyState(_model.State);
        }

        public BuildingModel Model => _model;
        public bool IsDestroyed => _view == null;

        public void Tick(float deltaTime)
        {
            _model.TickConstruction(deltaTime);
            _defenseController?.Tick(deltaTime);
        }

        public void Dispose()
        {
            _model.StateChanged -= OnStateChanged;
            _minefieldController?.Dispose();
        }

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
