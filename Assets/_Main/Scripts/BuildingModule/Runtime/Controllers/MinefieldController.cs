using System;
using System.Collections.Generic;
using ComponentsModule;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BuildingModule
{
    public sealed class MinefieldController : IDisposable
    {
        private readonly BuildingView _buildingView;
        private readonly BuildingModel _buildingModel;
        private readonly MinefieldModel _minefieldModel;
        private readonly MinefieldView _minefieldView;
        private readonly BuildingConfig _config;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly Dictionary<IEntity, int> _overlapCounts = new();

        public MinefieldController(
            BuildingView buildingView,
            BuildingModel buildingModel,
            MinefieldModel minefieldModel,
            MinefieldView minefieldView,
            BuildingConfig config,
            IBuildingRegistry buildingRegistry)
        {
            _buildingView = buildingView;
            _buildingModel = buildingModel;
            _minefieldModel = minefieldModel;
            _minefieldView = minefieldView;
            _config = config;
            _buildingRegistry = buildingRegistry;

            _minefieldView.TargetEntered += OnTargetEntered;
            _minefieldView.TargetExited += OnTargetExited;
            _buildingModel.StateChanged += OnBuildingStateChanged;

            UpdateTriggerState();
        }

        public void Dispose()
        {
            _minefieldView.TargetEntered -= OnTargetEntered;
            _minefieldView.TargetExited -= OnTargetExited;
            _buildingModel.StateChanged -= OnBuildingStateChanged;
            _overlapCounts.Clear();
        }

        private void OnTargetEntered(Collider targetCollider)
        {
            if (_buildingModel.State != BuildingState.Active || !_minefieldModel.HasMines)
                return;

            if (!TryGetTarget(targetCollider, out var entity, out var damageable))
                return;

            if (_overlapCounts.TryGetValue(entity, out var overlapCount))
            {
                _overlapCounts[entity] = overlapCount + 1;
                return;
            }

            _overlapCounts[entity] = 1;

            if (!_minefieldModel.TryConsumeMine())
                return;

            var targetPosition = targetCollider.bounds.center;
            var explosionPosition = targetPosition;

            _minefieldView.TryConsumeNearestMine(targetPosition, out explosionPosition);

            damageable.TakeDamage(_config.MineDamage, explosionPosition);
            _minefieldView.PlayExplosion(explosionPosition);

            if (!_minefieldModel.HasMines)
                Deplete();
        }

        private void OnTargetExited(Collider targetCollider)
        {
            var entity = targetCollider.GetComponentInParent<IEntity>();

            if (entity == null || !_overlapCounts.TryGetValue(entity, out var overlapCount))
                return;

            if (overlapCount <= 1)
            {
                _overlapCounts.Remove(entity);
                return;
            }

            _overlapCounts[entity] = overlapCount - 1;
        }

        private void OnBuildingStateChanged(BuildingState state) => UpdateTriggerState();

        private bool TryGetTarget(
            Collider targetCollider,
            out IEntity entity,
            out IDamageable damageable)
        {
            entity = targetCollider.GetComponentInParent<IEntity>();
            damageable = null;

            if (entity == null || !IsTargetLayer(targetCollider, entity))
                return false;

            if (!entity.TryGet(out damageable) || !damageable.IsAlive)
                return false;

            return true;
        }

        private bool IsTargetLayer(Collider targetCollider, IEntity entity)
        {
            if (IsLayerAllowed(targetCollider.gameObject.layer))
                return true;

            if (entity is not Component component)
                return false;

            if (IsLayerAllowed(component.gameObject.layer))
                return true;

            return IsLayerAllowed(component.transform.root.gameObject.layer);
        }

        private bool IsLayerAllowed(int layer) =>
            (_config.MineTargetLayers.value & (1 << layer)) != 0;

        private void UpdateTriggerState()
        {
            var isEnabled =
                _buildingModel.State == BuildingState.Active &&
                _minefieldModel.HasMines;

            _minefieldView.SetTriggerEnabled(isEnabled);

            if (!isEnabled)
                _overlapCounts.Clear();
        }

        private void Deplete()
        {
            _minefieldView.SetTriggerEnabled(false);
            _buildingRegistry.UnregisterBuilding(_buildingView);
            Object.Destroy(_buildingView.gameObject);
        }
    }
}
