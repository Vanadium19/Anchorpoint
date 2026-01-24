using System;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class PlacementServiceController : IInitializable, ITickable, IDisposable
    {
        private readonly IConstructionModeService _constructionModeService;
        private readonly IPlacementService _placementService;

        private readonly Camera _camera;
        private readonly int _layers;

        private bool _isActive;

        public PlacementServiceController(IConstructionModeService constructionModeService,
            IPlacementService placementService,
            LayerMask layers)
        {
            _constructionModeService = constructionModeService;
            _placementService = placementService;
            _layers = layers;
            _camera = Camera.main;
        }

        public void Initialize() => _constructionModeService.ActiveChanged += OnActiveChanged;

        public void Tick()
        {
            if (!_isActive)
                return;

            if (Input.GetMouseButtonDown(1))
                _placementService.Cancel();

            if (!TryGetPosition(out var position))
                return;

            _placementService.UpdatePosition(position);

            if (Input.GetMouseButtonDown(0))
                _placementService.Build(position);
        }

        public void Dispose() => _constructionModeService.ActiveChanged -= OnActiveChanged;

        public void SetActive(bool value)
        {
            if (_isActive == value)
                return;

            _isActive = value;

            if (!_isActive)
                _placementService.Cancel();
        }

        private bool TryGetPosition(out Vector3 position)
        {
            position = default;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, 100f, _layers))
                return false;

            position = hit.point;
            return true;
        }

        private void OnActiveChanged(bool isActive) => SetActive(isActive);
    }
}