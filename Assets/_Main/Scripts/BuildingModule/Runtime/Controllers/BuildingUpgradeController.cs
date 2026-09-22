using System;
using BaseModule;
using InputModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingUpgradeController : IInitializable, ITickable, IDisposable, IPausable
    {
        private static readonly Vector3 ViewportCenter = new(0.5f, 0.5f);

        private readonly IConstructionModeService _constructionModeService;
        private readonly IBuildingUpgradeService _upgradeService;
        private readonly IInputMap _inputMap;
        private readonly Camera _camera;
        private readonly PlacementConfig _placementConfig;
        private readonly IPauseManager _pauseManager;

        private Collider _hoveredCollider;
        private BuildingView _hoveredBuilding;
        private bool _isActive;
        private bool _isPaused;
        private bool _isUpgradeable;

        public BuildingUpgradeController(
            IConstructionModeService constructionModeService,
            IBuildingUpgradeService upgradeService,
            IInputMap inputMap,
            Camera camera,
            PlacementConfig placementConfig,
            IPauseManager pauseManager)
        {
            _constructionModeService = constructionModeService;
            _upgradeService = upgradeService;
            _inputMap = inputMap;
            _camera = camera;
            _placementConfig = placementConfig;
            _pauseManager = pauseManager;
        }

        public void Initialize()
        {
            _pauseManager.Register(this);
            _constructionModeService.ActiveChanged += OnConstructionModeChanged;
            _isActive = _constructionModeService.IsActive;
        }

        public void Tick()
        {
            if (_isPaused || !_isActive)
                return;

            UpdateHoveredBuilding();

            if (_inputMap.IsBuildUpgradePressed)
                TryUpgradeHoveredBuilding();
        }

        public void Dispose()
        {
            ClearHoveredBuilding();
            _constructionModeService.ActiveChanged -= OnConstructionModeChanged;
            _pauseManager.Unregister(this);
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            if (isPaused)
                ClearHoveredBuilding();
        }

        private void UpdateHoveredBuilding()
        {
            var building = GetBuildingUnderCursor();

            if (building == _hoveredBuilding)
                return;

            SetHoveredBuilding(building);
        }

        private void TryUpgradeHoveredBuilding()
        {
            if (!_isUpgradeable || _hoveredBuilding == null)
                return;

            _upgradeService.TryUpgrade(_hoveredBuilding);
            UpdateHoveredBuildingState();
        }

        private BuildingView GetBuildingUnderCursor()
        {
            if (_camera == null)
                return null;

            var ray = _camera.ViewportPointToRay(ViewportCenter);

            if (!Physics.Raycast(ray, out var hit, _placementConfig.MaxPlacementDistance))
            {
                _hoveredCollider = null;
                return null;
            }

            if (hit.collider == _hoveredCollider)
                return _hoveredBuilding;

            _hoveredCollider = hit.collider;
            return hit.collider.GetComponentInParent<BuildingView>();
        }

        private void SetHoveredBuilding(BuildingView building)
        {
            if (_hoveredBuilding != null)
                _hoveredBuilding.SetUpgradeHighlight(BuildingUpgradeHighlightState.None);

            _hoveredBuilding = building;
            UpdateHoveredBuildingState();
        }

        private void UpdateHoveredBuildingState()
        {
            _isUpgradeable = false;

            if (_hoveredBuilding == null)
                return;

            if (!_upgradeService.TryGetInfo(_hoveredBuilding, out var info) || !info.CanUpgrade)
            {
                _hoveredBuilding.SetUpgradeHighlight(BuildingUpgradeHighlightState.None);
                return;
            }

            _isUpgradeable = true;
            var state = info.CanAfford
                ? BuildingUpgradeHighlightState.Available
                : BuildingUpgradeHighlightState.Unavailable;
            _hoveredBuilding.SetUpgradeHighlight(state);
        }

        private void ClearHoveredBuilding()
        {
            _hoveredCollider = null;
            _isUpgradeable = false;

            if (_hoveredBuilding != null)
                _hoveredBuilding.SetUpgradeHighlight(BuildingUpgradeHighlightState.None);

            _hoveredBuilding = null;
        }

        private void OnConstructionModeChanged(bool isActive)
        {
            _isActive = isActive;

            if (!isActive)
                ClearHoveredBuilding();
        }
    }
}
