using System;
using BaseModule;
using ComponentsModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class PlacementController : IInitializable, ITickable, IDisposable, IPausable
    {
        private static readonly Vector3 ViewportCenter = new(0.5f, 0.5f);

        private readonly IConstructionModeService _constructionModeService;
        private readonly IPlacementService _placementService;
        private readonly IGrid _grid;
        private readonly IPlacementInputHandler _inputHandler;
        private readonly IPreviewService _previewService;
        private readonly PlacementConfig _config;
        private readonly BuildingCatalog _catalog;
        private readonly Camera _camera;
        private readonly LayerMask _raycastLayers;
        private readonly bool _useGrid;
        private readonly IPauseManager _pauseManager;

        private bool _isActive;
        private float _relativeRotation;
        private float _currentRotation;
        private float _currentPlacementDistance;
        private Vector3 _smoothedPosition;
        private Vector3 _targetPosition;
        private bool _hasGroundSupport;
        private bool _isPaused;

        public PlacementController(
            IConstructionModeService constructionModeService,
            IPlacementService placementService,
            IGrid grid,
            IPlacementInputHandler inputHandler,
            IPreviewService previewService,
            PlacementConfig config,
            BuildingCatalog catalog,
            Camera camera,
            IPauseManager pauseManager,
            LayerMask raycastLayers = default,
            bool useGrid = true)
        {
            _constructionModeService = constructionModeService;
            _placementService = placementService;
            _grid = grid;
            _inputHandler = inputHandler;
            _previewService = previewService;
            _config = config;
            _catalog = catalog;
            _camera = camera;
            _pauseManager = pauseManager;
            _raycastLayers = raycastLayers;
            _useGrid = useGrid;
            _currentPlacementDistance = config.MaxPlacementDistance;
        }

        public void Initialize()
        {
            _pauseManager.Register(this);
            _constructionModeService.ActiveChanged += OnActiveChanged;
        }

        public void Tick()
        {
            if (_isPaused || !_isActive)
                return;

            HandleRotation();
            HandleScroll();
            UpdatePosition();
            UpdateRotation();
            HandlePlace();
        }

        public void Dispose()
        {
            _constructionModeService.ActiveChanged -= OnActiveChanged;
            _pauseManager.Unregister(this);
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            if (isPaused)
                _placementService.Cancel();
        }

        private void HandleRotation()
        {
            var rotationDelta = _inputHandler.RotationDelta;

            if (Mathf.Abs(rotationDelta) < _config.InputTolerance)
                return;

            var oldRotation = _relativeRotation;
            _relativeRotation += rotationDelta;

            if (Mathf.Abs(_relativeRotation - oldRotation) > _config.InputTolerance)
                UpdateRotation();
        }

        private void HandleScroll()
        {
            var scrollDelta = _inputHandler.ScrollDelta;

            if (Mathf.Abs(scrollDelta) < _config.InputTolerance)
                return;

            _currentPlacementDistance += scrollDelta;
            _currentPlacementDistance = Mathf.Clamp(_currentPlacementDistance, _config.MinPlacementDistance, _config.MaxPlacementDistance);
        }

        private void UpdateRotation()
        {
            var cameraYaw = _camera.transform.eulerAngles.y;
            _currentRotation = cameraYaw + _relativeRotation;
            _placementService.UpdateRotation(_currentRotation);
        }

        private void UpdatePosition()
        {
            if (!TryGetRaycastPosition(out var targetPosition))
                return;

            _targetPosition = targetPosition;

            if (_useGrid)
            {
                if (_grid.TryGetNearestTile(targetPosition, out var tile))
                    _placementService.UpdatePosition(tile.WorldPosition);
            }
            else
            {
                _smoothedPosition = Vector3.Lerp(_smoothedPosition, targetPosition, _config.SmoothSpeed);
                _placementService.UpdatePositionFree(_smoothedPosition, _hasGroundSupport);
            }
        }

        private bool TryGetRaycastPosition(out Vector3 position)
        {
            position = default;

            var cameraForward = _camera.transform.forward;
            var playerPosition = _camera.transform.position;
            var targetPosition = playerPosition + cameraForward * _currentPlacementDistance;

            var ray = _camera.ViewportPointToRay(ViewportCenter);

            if (Physics.Raycast(ray, out var hit, _currentPlacementDistance, _raycastLayers))
            {
                _hasGroundSupport = CheckTargetLayer(hit);
                
                position = hit.point;
                
                if (_previewService.TryGetCollisionBounds(position, _currentRotation, out _, out var halfSize, out _))
                {
                    var isFloor = hit.normal.y > 0.99f;
                    var isCeiling = hit.normal.y < -0.5f;
                    
                    if (isCeiling)
                    {
                        var dot = Vector3.Dot(hit.normal, Vector3.down);
                        var angleOffset = _config.CeilingAngleFactor * (1f - dot);
                        position += Vector3.down * (halfSize.y * 2f + _config.CeilingOffset + angleOffset);
                    }
                    else if (!isFloor)
                    {
                        var rotation = Quaternion.Euler(0, _currentRotation, 0);
                        var right = rotation * Vector3.right;
                        var forward = rotation * Vector3.forward;
                        var depth = Mathf.Abs(Vector3.Dot(hit.normal, right)) * halfSize.x + 
                                    Mathf.Abs(Vector3.Dot(hit.normal, forward)) * halfSize.z;
                        
                        var dot = Vector3.Dot(hit.normal, Vector3.up);
                        var isOutsideAngle = dot < 0;
                        var angleOffset = isOutsideAngle ? _config.WallAngleFactor * Mathf.Abs(dot) : 0;
                        position += hit.normal * (depth + _config.WallOffset + angleOffset);
                    }
                }
                else
                {
                    _hasGroundSupport = true;
                }
                
                position.y += _config.PreviewYOffset;
                return true;
            }

            position = targetPosition;
            _hasGroundSupport = false;
            return true;
        }

        private void HandlePlace()
        {
            if (_inputHandler.IsPlacePressed)
            {
                if (!_hasGroundSupport)
                    return;

                var position = _placementService.LastValidPosition;
                _placementService.Build(position, _useGrid);
            }

            if (_inputHandler.IsCancelPressed)
                _constructionModeService.SetActive(false);
        }

        private bool CheckTargetLayer(RaycastHit hit)
        {
            if (!_catalog.TryGetConfig(_placementService.CurrentBuildingId, out var buildingConfig))
                return true;
                
            var hitLayer = hit.collider.gameObject.layer;
            return (buildingConfig.AllowedBuildLayers & (1 << hitLayer)) != 0;
        }

        private void OnActiveChanged(bool isActive)
        {
            if (_isActive == isActive)
                return;

            _isActive = isActive;

            if (_isActive)
            {
                _relativeRotation = 0f;
                _currentRotation = 0f;
                _currentPlacementDistance = _config.MaxPlacementDistance;
                _smoothedPosition = Vector3.zero;
                _hasGroundSupport = true;
                _placementService.SetBuilding(null);
            }
            else
            {
                _placementService.Cancel();
            }
        }
    }
}
