using System;
using UnityEngine;

namespace BuildingModule
{
    public class PlacementService : IPlacementService
    {
        private readonly IGrid _grid;
        private readonly IPreviewService _previewService;
        private readonly IStorageService _storage;
        private readonly BuildingFactory _factory;
        private readonly BuildingCatalog _catalog;
        private readonly PlacementConfig _config;

        private string _currentBuildingId;
        private Vector3 _lastValidPosition;
        private float _currentRotation;

        public event Action<string> BuildingChanged;
        public event Action PlacementCompleted;
        public event Action SelectionCleared;

        public PlacementService(
            IGrid grid,
            IPreviewService previewService,
            IStorageService storage,
            BuildingFactory factory,
            BuildingCatalog catalog,
            PlacementConfig config)
        {
            _grid = grid;
            _previewService = previewService;
            _storage = storage;
            _factory = factory;
            _catalog = catalog;
            _config = config;
        }

        public string CurrentBuildingId => _currentBuildingId;

        public Vector3 LastValidPosition => _lastValidPosition;

        public float CurrentRotation => _currentRotation;

        public bool HasCollisionAtPosition(Vector3 position)
        {
            return _previewService.HasCollisionAtPosition(position, _currentRotation);
        }

        public void SetBuilding(string id)
        {
            if (_currentBuildingId == id)
                return;

            _currentBuildingId = id;
            _previewService.SetPreview(id);
            _previewService.UpdatePreview(_currentRotation);
            BuildingChanged?.Invoke(id);
        }

        public void UpdatePosition(Vector3 worldPosition)
        {
            if (string.IsNullOrEmpty(_currentBuildingId))
                return;

            if (!_grid.TryGetNearestTile(worldPosition, out var tile))
                return;

            _lastValidPosition = tile.WorldPosition;
            _previewService.UpdatePreview(tile.WorldPosition, tile.IsOccupied, _currentRotation);
        }

        public void UpdatePositionFree(Vector3 worldPosition, bool hasGroundSupport)
        {
            if (string.IsNullOrEmpty(_currentBuildingId))
                return;

            _lastValidPosition = worldPosition;

            var hasCollision = _previewService.HasCollisionAtPosition(worldPosition, _currentRotation);
            _previewService.UpdatePreview(worldPosition, hasCollision, hasGroundSupport);
        }

        public void UpdateRotation(float rotation)
        {
            if (Mathf.Abs(_currentRotation - rotation) < 0.01f)
                return;

            _currentRotation = rotation;
            _previewService.UpdatePreview(_currentRotation);
        }

        public bool Build(Vector3 worldPosition, bool useGrid = true)
        {
            if (string.IsNullOrEmpty(_currentBuildingId))
                return false;

            if (useGrid)
            {
                if (!_grid.TryGetNearestTile(worldPosition, out var tile))
                    return false;

                if (tile.IsOccupied)
                    return false;

                if (!_storage.CanBuy(_currentBuildingId))
                    return false;

                var rotation = Quaternion.Euler(0f, _currentRotation, 0f);
                _factory.Create(_currentBuildingId, tile.WorldPosition, rotation);
                _storage.Buy(_currentBuildingId);
                tile.Occupy();
            }
            else
            {
                if (HasCollisionAtPosition(worldPosition))
                    return false;

                if (!_storage.CanBuy(_currentBuildingId))
                    return false;

                var rotation = Quaternion.Euler(0f, _currentRotation, 0f);
                _factory.Create(_currentBuildingId, worldPosition, rotation);
                _storage.Buy(_currentBuildingId);
            }

            _previewService.Cancel();
            _previewService.SetPreview(_currentBuildingId);
            PlacementCompleted?.Invoke();
            return true;
        }

        public void Cancel()
        {
            _currentBuildingId = null;
            _previewService.Cancel();
            SelectionCleared?.Invoke();
        }
    }
}