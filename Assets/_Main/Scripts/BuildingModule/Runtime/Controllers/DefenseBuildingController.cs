using ComponentsModule;
using UnityEngine;

namespace BuildingModule
{
    public sealed class DefenseBuildingController
    {
        private const float TargetSearchInterval = 0.2f;

        private readonly BuildingView _view;
        private readonly BuildingModel _model;
        private readonly BuildingConfig _config;
        private readonly ICombatTargetSource _targetSource;
        private readonly DefenseBuildingView _defenseView;

        private float _attackCooldown;

        public DefenseBuildingController(
            BuildingView view,
            BuildingModel model,
            BuildingConfig config,
            ICombatTargetSource targetSource)
        {
            _view = view;
            _model = model;
            _config = config;
            _targetSource = targetSource;
            _defenseView = view.DefenseView;
        }

        public void Tick(float deltaTime)
        {
            if (_model.State != BuildingState.Active)
            {
                _attackCooldown = 0f;
                return;
            }

            if (_targetSource == null)
                return;

            _attackCooldown -= deltaTime;

            if (_attackCooldown > 0f)
                return;

            if (!_targetSource.TryGetNearest(
                    _view.transform.position,
                    _config.DefenseSearchRadius,
                    out var target))
            {
                _attackCooldown = TargetSearchInterval;
                return;
            }

            _defenseView?.Attack(target.Position);
            target.TakeDamage(_config.DefenseDamage);

            var attackRate = Mathf.Max(_config.DefenseAttackRate, 0.01f);
            _attackCooldown = 1f / attackRate;
        }
    }
}
