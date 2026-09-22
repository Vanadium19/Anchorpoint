using System;
using BaseModule;
using InputModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingUpgradeController : IInitializable, ITickable, IDisposable, IPausable
    {
        private const float InfoRefreshSeconds = 0.25f;

        private static readonly Vector3 ViewportCenter = new(0.5f, 0.5f);

        private readonly IConstructionModeService _constructionModeService;
        private readonly IBuildingUpgradeService _upgradeService;
        private readonly IInputMap _inputMap;
        private readonly Camera _camera;
        private readonly PlacementConfig _placementConfig;
        private readonly IPauseManager _pauseManager;
        private readonly BuildingUpgradeHoldModel _holdModel = new();

        private Collider _hoveredCollider;
        private BuildingView _hoveredBuilding;
        private bool _isActive;
        private bool _isPaused;
        private bool _isUpgradeable;
        private bool _canAfford;
        private float _refreshSeconds;

        public event Action<BuildingUpgradeInfo> UpgradeInfoChanged;

        public event Action<float> UpgradeProgressChanged;

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

            if (!_isUpgradeable)
                return;

            _refreshSeconds += Time.deltaTime;

            if (_refreshSeconds >= InfoRefreshSeconds)
            {
                _refreshSeconds = 0f;
                UpdateHoveredBuildingState();
            }

            UpdateUpgradeHold();
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

        private void UpdateUpgradeHold()
        {
            var previousProgress = _holdModel.Progress;
            var isComplete = _holdModel.Advance(_inputMap.IsBuildUpgradeHeld, _canAfford, Time.deltaTime);

            if (!Mathf.Approximately(previousProgress, _holdModel.Progress))
                UpgradeProgressChanged?.Invoke(_holdModel.Progress);

            if (!isComplete)
                return;

            _upgradeService.TryUpgrade(_hoveredBuilding);
            ResetUpgradeHold();
            UpdateHoveredBuildingState();
        }

        private void ResetUpgradeHold()
        {
            var previousProgress = _holdModel.Progress;
            _holdModel.Reset(_inputMap.IsBuildUpgradeHeld);

            if (previousProgress > 0f)
                UpgradeProgressChanged?.Invoke(0f);
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
                _hoveredBuilding.SetUpgradeHighlight(BuildingUpgradeHighlightState.None, _placementConfig.PreviewAlpha);

            _hoveredBuilding = building;
            _refreshSeconds = 0f;
            ResetUpgradeHold();
            UpdateHoveredBuildingState();
        }

        private void UpdateHoveredBuildingState()
        {
            _isUpgradeable = false;
            _canAfford = false;

            if (_hoveredBuilding == null)
            {
                UpgradeInfoChanged?.Invoke(null);
                return;
            }

            if (!_upgradeService.TryGetInfo(_hoveredBuilding, out var info) || !info.CanUpgrade)
            {
                _hoveredBuilding.SetUpgradeHighlight(BuildingUpgradeHighlightState.None, _placementConfig.PreviewAlpha);
                UpgradeInfoChanged?.Invoke(null);
                ResetUpgradeHold();
                return;
            }

            _isUpgradeable = true;
            _canAfford = info.CanAfford;
            var state = info.CanAfford
                ? BuildingUpgradeHighlightState.Available
                : BuildingUpgradeHighlightState.Unavailable;
            _hoveredBuilding.SetUpgradeHighlight(state, _placementConfig.PreviewAlpha, info.NextVisualPrefab);
            UpgradeInfoChanged?.Invoke(info);

            if (!_canAfford)
                ResetUpgradeHold();
        }

        private void ClearHoveredBuilding()
        {
            _hoveredCollider = null;
            _isUpgradeable = false;
            _canAfford = false;
            ResetUpgradeHold();

            if (_hoveredBuilding != null)
                _hoveredBuilding.SetUpgradeHighlight(BuildingUpgradeHighlightState.None, _placementConfig.PreviewAlpha);

            _hoveredBuilding = null;
            UpgradeInfoChanged?.Invoke(null);
        }

        private void OnConstructionModeChanged(bool isActive)
        {
            _isActive = isActive;

            if (!isActive)
                ClearHoveredBuilding();
        }
    }
}
