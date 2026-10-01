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
        private const float MinPreviewAlpha = 0.1f;

        private static readonly Vector3 ViewportCenter = new(0.5f, 0.5f);

        private readonly IConstructionModeService _constructionModeService;
        private readonly IPlacementService _placementService;
        private readonly IBuildingUpgradeService _upgradeService;
        private readonly IInputMap _inputMap;
        private readonly Camera _camera;
        private readonly PlacementConfig _placementConfig;
        private readonly IPauseManager _pauseManager;
        private readonly BuildingUpgradeHoldModel _holdModel = new();

        private Collider _hoveredCollider;
        private BuildingView _hoveredBuilding;
        private BuildingUpgradeInfo _hoveredInfo;
        private bool _isActive;
        private bool _isPaused;
        private float _refreshSeconds;

        public event Action<BuildingUpgradeInfo> UpgradeInfoChanged;

        public event Action<float> UpgradeProgressChanged;

        public event Action UpgradeDenied;

        public event Action BuildingUpgraded;

        public BuildingUpgradeController(
            IConstructionModeService constructionModeService,
            IPlacementService placementService,
            IBuildingUpgradeService upgradeService,
            IInputMap inputMap,
            Camera camera,
            PlacementConfig placementConfig,
            IPauseManager pauseManager)
        {
            _constructionModeService = constructionModeService;
            _placementService = placementService;
            _upgradeService = upgradeService;
            _inputMap = inputMap;
            _camera = camera;
            _placementConfig = placementConfig;
            _pauseManager = pauseManager;
        }

        private bool CanUpgradeHovered => _hoveredInfo is { CanUpgrade: true };

        private bool CanAffordHovered => _hoveredInfo is { CanUpgrade: true, CanAfford: true };

        public void Initialize()
        {
            _pauseManager.Register(this);
            _constructionModeService.ActiveChanged += OnConstructionModeChanged;
            _placementService.BuildingChanged += OnPlacementBuildingChanged;
            _placementService.SelectionCleared += OnPlacementSelectionCleared;
            RefreshActive();
        }

        public void Tick()
        {
            if (!_isActive)
                return;

            UpdateHoveredBuilding();

            if (_hoveredBuilding == null)
                return;

            _refreshSeconds += Time.deltaTime;

            if (_refreshSeconds >= InfoRefreshSeconds)
            {
                _refreshSeconds = 0f;
                UpdateHoveredBuildingState();
            }

            if (!CanUpgradeHovered)
                return;

            HandleUpgradeDenied();
            UpdateUpgradeHold();
            UpdateUpgradePreview();
        }

        public void Dispose()
        {
            ClearHoveredBuilding();
            _constructionModeService.ActiveChanged -= OnConstructionModeChanged;
            _placementService.BuildingChanged -= OnPlacementBuildingChanged;
            _placementService.SelectionCleared -= OnPlacementSelectionCleared;
            _pauseManager.Unregister(this);
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            RefreshActive();
        }

        private void RefreshActive()
        {
            var isActive = !_isPaused
                && _constructionModeService.IsActive
                && string.IsNullOrEmpty(_placementService.CurrentBuildingId);

            if (_isActive == isActive)
                return;

            _isActive = isActive;

            if (!_isActive)
                ClearHoveredBuilding();
        }

        private void UpdateHoveredBuilding()
        {
            var building = GetBuildingUnderCursor();

            if (building != _hoveredBuilding)
                SetHoveredBuilding(building);
        }

        private void HandleUpgradeDenied()
        {
            if (_inputMap.IsBuildUpgradePressed && !CanAffordHovered)
                UpgradeDenied?.Invoke();
        }

        private void UpdateUpgradeHold()
        {
            var previousProgress = _holdModel.Progress;
            var isComplete = _holdModel.Advance(_inputMap.IsBuildUpgradeHeld, CanAffordHovered, Time.deltaTime);

            if (!Mathf.Approximately(previousProgress, _holdModel.Progress))
                UpgradeProgressChanged?.Invoke(_holdModel.Progress);

            if (!isComplete)
                return;

            var building = _hoveredBuilding;
            var isUpgraded = _upgradeService.TryUpgrade(building);
            ResetUpgradeHold();
            UpdateHoveredBuildingState();

            if (!isUpgraded)
                return;

            building.PlayUpgradeAnimation();
            BuildingUpgraded?.Invoke();
        }

        private void UpdateUpgradePreview()
        {
            if (_hoveredBuilding == null || !CanUpgradeHovered)
                return;

            var isPreviewVisible = _inputMap.IsBuildUpgradeHeld && !_holdModel.IsAwaitingRelease;
            var alpha = 0f;

            if (isPreviewVisible)
            {
                alpha = CanAffordHovered
                    ? Mathf.Lerp(MinPreviewAlpha, _placementConfig.PreviewAlpha, _holdModel.Progress)
                    : _placementConfig.PreviewAlpha;
            }

            _hoveredBuilding.RenderUpgradePreview(_hoveredInfo.NextVisualPrefab, GetHighlightState(), alpha);
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

            if (!Physics.Raycast(ray, out var hit, _placementConfig.MaxUpgradeDistance))
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
            ResetHoveredBuildingVisual();
            _hoveredBuilding = building;
            _refreshSeconds = 0f;
            ResetUpgradeHold();
            UpdateHoveredBuildingState();
        }

        private void UpdateHoveredBuildingState()
        {
            _hoveredInfo = null;

            if (_hoveredBuilding == null)
            {
                UpgradeInfoChanged?.Invoke(null);
                return;
            }

            if (!_upgradeService.TryGetInfo(_hoveredBuilding, out var info))
            {
                ResetHoveredBuildingVisual();
                UpgradeInfoChanged?.Invoke(null);
                return;
            }

            _hoveredInfo = info;

            if (!CanUpgradeHovered)
            {
                ResetHoveredBuildingVisual();
                ResetUpgradeHold();
            }
            else
            {
                _hoveredBuilding.RenderUpgradeHighlight(GetHighlightState());
            }

            UpgradeInfoChanged?.Invoke(info);
        }

        private BuildingUpgradeHighlightState GetHighlightState()
        {
            return CanAffordHovered
                ? BuildingUpgradeHighlightState.Available
                : BuildingUpgradeHighlightState.Unavailable;
        }

        private void ResetHoveredBuildingVisual()
        {
            if (_hoveredBuilding == null)
                return;

            _hoveredBuilding.RenderUpgradeHighlight(BuildingUpgradeHighlightState.None);
            _hoveredBuilding.RenderUpgradePreview(null, BuildingUpgradeHighlightState.None, 0f);
        }

        private void ClearHoveredBuilding()
        {
            _hoveredCollider = null;
            ResetUpgradeHold();
            ResetHoveredBuildingVisual();
            _hoveredBuilding = null;
            _hoveredInfo = null;
            UpgradeInfoChanged?.Invoke(null);
        }

        private void OnConstructionModeChanged(bool isActive) => RefreshActive();

        private void OnPlacementBuildingChanged(string buildingId) => RefreshActive();

        private void OnPlacementSelectionCleared() => RefreshActive();
    }
}
