using UnityEngine;

namespace BuildingModule
{
    public class PreviewService : IPreviewService
    {
        private readonly BuildingFactory _factory;

        private BuildingView _preview;

        public PreviewService(BuildingFactory factory)
        {
            _factory = factory;
        }

        public void SetPreview(BuildingName value)
        {
            if (_preview?.Name == value)
                return;

            if (_preview)
                DestroyPreview();

            if (value == BuildingName.None)
                return;

            _preview = _factory.Create(value, Vector3.zero, Quaternion.identity);

            if (!_preview)
                return;

            UpdateMaterial(false);
        }

        public void UpdatePreview(Vector3 position, bool isOccupied)
        {
            position.y += 0.1f;
            _preview.transform.position = position;
            UpdateMaterial(isOccupied);
        }

        public void Cancel() => DestroyPreview();

        private void DestroyPreview()
        {
            Object.Destroy(_preview.gameObject);
            _preview = null;
        }

        private void UpdateMaterial(bool isOccupied)
        {
            var color = isOccupied ? Color.red : Color.green;
            _preview.MeshRenderer.material.color = color;
        }
    }
}