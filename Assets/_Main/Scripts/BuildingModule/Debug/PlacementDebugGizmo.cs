using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class PlacementDebugGizmo : MonoBehaviour, IInitializable
    {
        [Inject]
        private IPlacementService _placementService;

        [Inject]
        private IPreviewService _previewService;

        [Inject]
        private PlacementConfig _placementConfig;

        [SerializeField] private BuildingCatalog catalog;
        [SerializeField] private bool showDebug = true;

        private LineRenderer _lineRenderer;

        public void Initialize() { }

        private void Start()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.positionCount = 5;
            _lineRenderer.loop = true;
            _lineRenderer.widthMultiplier = 0.05f;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void Update()
        {
            if (_placementService == null || _lineRenderer == null)
                return;

            _lineRenderer.enabled = showDebug;

            if (!showDebug)
                return;

            var placementRotation = _placementService.CurrentRotation;
            var basePosition = _placementService.LastValidPosition + Vector3.up * _placementConfig.PreviewYOffset;

            if (!_previewService.TryGetCollisionBounds(basePosition, placementRotation, out var worldCenter, out var halfSize, out var totalRotation))
            {
                _lineRenderer.enabled = false;
                return;
            }

            var hasCollision = _placementService.HasCollisionAtPosition(basePosition);
            _lineRenderer.startColor = hasCollision ? Color.red : Color.green;
            _lineRenderer.endColor = hasCollision ? Color.red : Color.green;

            var bottomFrontLeft = totalRotation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            var bottomFrontRight = totalRotation * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            var bottomBackRight = totalRotation * new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            var bottomBackLeft = totalRotation * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            var topFrontLeft = totalRotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            var topFrontRight = totalRotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            var topBackRight = totalRotation * new Vector3(halfSize.x, halfSize.y, halfSize.z);
            var topBackLeft = totalRotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            _lineRenderer.positionCount = 24;
            _lineRenderer.loop = false;

            int i = 0;
            _lineRenderer.SetPosition(i++, worldCenter + bottomFrontLeft);
            _lineRenderer.SetPosition(i++, worldCenter + bottomFrontRight);
            _lineRenderer.SetPosition(i++, worldCenter + bottomFrontRight);
            _lineRenderer.SetPosition(i++, worldCenter + bottomBackRight);
            _lineRenderer.SetPosition(i++, worldCenter + bottomBackRight);
            _lineRenderer.SetPosition(i++, worldCenter + bottomBackLeft);
            _lineRenderer.SetPosition(i++, worldCenter + bottomBackLeft);
            _lineRenderer.SetPosition(i++, worldCenter + bottomFrontLeft);

            _lineRenderer.SetPosition(i++, worldCenter + topFrontLeft);
            _lineRenderer.SetPosition(i++, worldCenter + topFrontRight);
            _lineRenderer.SetPosition(i++, worldCenter + topFrontRight);
            _lineRenderer.SetPosition(i++, worldCenter + topBackRight);
            _lineRenderer.SetPosition(i++, worldCenter + topBackRight);
            _lineRenderer.SetPosition(i++, worldCenter + topBackLeft);
            _lineRenderer.SetPosition(i++, worldCenter + topBackLeft);
            _lineRenderer.SetPosition(i++, worldCenter + topFrontLeft);

            _lineRenderer.SetPosition(i++, worldCenter + bottomFrontLeft);
            _lineRenderer.SetPosition(i++, worldCenter + topFrontLeft);
            _lineRenderer.SetPosition(i++, worldCenter + bottomFrontRight);
            _lineRenderer.SetPosition(i++, worldCenter + topFrontRight);
            _lineRenderer.SetPosition(i++, worldCenter + bottomBackRight);
            _lineRenderer.SetPosition(i++, worldCenter + topBackRight);
            _lineRenderer.SetPosition(i++, worldCenter + bottomBackLeft);
            _lineRenderer.SetPosition(i++, worldCenter + topBackLeft);
        }

        private void OnDrawGizmos()
        {
            if (_placementService == null || _placementConfig == null || !showDebug)
                return;

            var placementRotation = _placementService.CurrentRotation;
            var basePosition = _placementService.LastValidPosition + Vector3.up * _placementConfig.PreviewYOffset;

            if (!_previewService.TryGetCollisionBounds(basePosition, placementRotation, out var worldCenter, out var halfSize, out var totalRotation))
                return;

            var hasCollision = _placementService.HasCollisionAtPosition(basePosition);

            Gizmos.color = hasCollision ? Color.red : Color.green;

            Gizmos.matrix = Matrix4x4.TRS(worldCenter, totalRotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfSize * 2f);
        }
    }
}
