using System;
using System.Collections.Generic;
using ComponentsModule;
using UnityEngine;

namespace BuildingModule
{
    public sealed class WoodenStakesController : IDisposable
    {
        private readonly BuildingModel _buildingModel;
        private readonly WoodenStakesView _view;
        private readonly float _speedMultiplier;
        private readonly Dictionary<IEntity, int> _overlapCounts = new();
        private readonly Dictionary<IEntity, IPathMoveComponent> _movements = new();

        public WoodenStakesController(
            BuildingModel buildingModel,
            WoodenStakesView view,
            float speedMultiplier)
        {
            _buildingModel = buildingModel;
            _view = view;
            _speedMultiplier = speedMultiplier;

            _view.TargetEntered += OnTargetEntered;
            _view.TargetExited += OnTargetExited;
            _buildingModel.StateChanged += OnBuildingStateChanged;

            UpdateTriggerState();
        }

        public void Dispose()
        {
            _view.TargetEntered -= OnTargetEntered;
            _view.TargetExited -= OnTargetExited;
            _buildingModel.StateChanged -= OnBuildingStateChanged;

            ClearSlowdowns();
        }

        private void OnTargetEntered(Collider targetCollider)
        {
            if (_buildingModel.State != BuildingState.Active)
                return;

            var entity = targetCollider.GetComponentInParent<IEntity>();

            if (entity == null || !entity.TryGet<IPathMoveComponent>(out var movement))
                return;

            if (_overlapCounts.TryGetValue(entity, out var overlapCount))
            {
                _overlapCounts[entity] = overlapCount + 1;
                return;
            }

            _overlapCounts[entity] = 1;
            _movements[entity] = movement;

            movement.ApplySpeedMultiplier(this, _speedMultiplier);
        }

        private void OnTargetExited(Collider targetCollider)
        {
            var entity = targetCollider.GetComponentInParent<IEntity>();

            if (entity == null || !_overlapCounts.TryGetValue(entity, out var overlapCount))
                return;

            if (overlapCount > 1)
            {
                _overlapCounts[entity] = overlapCount - 1;
                return;
            }

            _overlapCounts.Remove(entity);

            if (!_movements.Remove(entity, out var movement))
                return;

            movement.RemoveSpeedMultiplier(this);
        }

        private void OnBuildingStateChanged(BuildingState state) => UpdateTriggerState();

        private void UpdateTriggerState()
        {
            var isActive = _buildingModel.State == BuildingState.Active;

            if (!isActive)
                ClearSlowdowns();

            _view.SetTriggerEnabled(isActive);
        }

        private void ClearSlowdowns()
        {
            foreach (var movement in _movements.Values)
                movement.RemoveSpeedMultiplier(this);

            _movements.Clear();
            _overlapCounts.Clear();
        }
    }
}
