using UnityEngine;

namespace BuildingModule
{
    public class PlacementService : IPlacementService
    {
        private readonly IGrid _grid;

        private readonly IPreviewService _previewService;
        private readonly BuildingFactory _factory;

        private BuildingName _currentBuilding;

        public PlacementService(IGrid grid, IPreviewService previewService, BuildingFactory factory)
        {
            _grid = grid;
            _previewService = previewService;
            _factory = factory;
        }

        public BuildingName CurrentBuilding => _currentBuilding;

        public void SetBuilding(BuildingName value)
        {
            if (_currentBuilding == value)
                return;

            _currentBuilding = value;
            _previewService.SetPreview(value);
        }

        public void UpdatePosition(Vector3 worldPosition)
        {
            if (_currentBuilding == BuildingName.None)
                return;

            if (!_grid.TryGetNearestTile(worldPosition, out var tile))
                return;

            _previewService.UpdatePreview(tile.WorldPosition, tile.IsOccupied);
        }

        public bool Build(Vector3 worldPosition)
        {
            if (_currentBuilding == BuildingName.None)
                return false;

            if (!_grid.TryGetNearestTile(worldPosition, out var tile))
                return false;

            if (tile.IsOccupied)
                return false;

            _factory.Create(_currentBuilding, tile.WorldPosition, Quaternion.identity);
            tile.Occupy();
            Cancel();
            return true;
        }

        public void Cancel() => SetBuilding(BuildingName.None);
    }
}