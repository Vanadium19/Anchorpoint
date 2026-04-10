using System.Collections.Generic;
using UnityEngine;

namespace BuildingModule
{
    public class PreviewService : IPreviewService
    {
        private readonly BuildingFactory _factory;
        private readonly PlacementConfig _config;

        private BuildingView _preview;
        private string _currentPreviewId;
        private List<Material> _previewMaterials = new List<Material>();
        private Quaternion _initialColliderRotation;
        private bool _canAfford = true;

        public PreviewService(BuildingFactory factory, PlacementConfig config)
        {
            _factory = factory;
            _config = config;
        }

        public bool HasCollisionAtPosition(Vector3 position, float rotation)
        {
            if (!TryGetCollisionBounds(position, rotation, out var worldCenter, out var halfSize, out var totalRotation))
                return false;

            return Physics.CheckBox(worldCenter, halfSize, totalRotation, _config.CollisionLayer, _config.QueryTriggerInteraction);
        }

        public bool TryGetCollisionBounds(Vector3 position, float rotation, out Vector3 worldCenter, out Vector3 halfSize, out Quaternion totalRotation)
        {
            worldCenter = Vector3.zero;
            halfSize = Vector3.one;
            totalRotation = Quaternion.identity;

            if (_preview == null)
                return false;

            var collider = _preview.CollisionCollider;

            if (collider == null)
                return false;

            if (collider is BoxCollider boxCollider)
            {
                var scale = collider.transform.lossyScale;
                var scaledSize = Vector3.Scale(boxCollider.size, scale);
                var placementRotation = Quaternion.Euler(0f, rotation, 0f);
                totalRotation = placementRotation * _initialColliderRotation;
                
                var localCenter = boxCollider.transform.localPosition + Vector3.Scale(boxCollider.center, scale);
                worldCenter = position + totalRotation * localCenter;
                halfSize = scaledSize / 2f;
                
                return true;
            }

            return false;
        }

        public void SetPreview(string id)
        {
            if (_currentPreviewId == id)
                return;

            if (_preview)
                DestroyPreview();

            if (string.IsNullOrEmpty(id))
                return;

            _currentPreviewId = id;
            _preview = _factory.Create(id, Vector3.zero, Quaternion.identity);

            if (!_preview)
                return;

            var collider = _preview.CollisionCollider;
            _initialColliderRotation = collider != null ? collider.transform.localRotation : Quaternion.identity;

            DisableColliders();
            CollectMaterials();
            DisableShadows();
            SetMaterialsTransparent();
            UpdateMaterial(false);
        }

        private void DisableShadows()
        {
            var renderers = _preview.GetAllMeshRenderers();
            foreach (var renderer in renderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private void CollectMaterials()
        {
            _previewMaterials.Clear();
            var renderers = _preview.GetAllMeshRenderers();
            foreach (var renderer in renderers)
            {
                var newMats = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMats[i] = new Material(renderer.materials[i]);
                    _previewMaterials.Add(newMats[i]);
                }
                renderer.materials = newMats;
            }
        }

        private void SetMaterialsTransparent()
        {
            foreach (var mat in _previewMaterials)
            {
                if (mat.HasProperty("_Surface"))
                    mat.SetFloat("_Surface", 1);

                if (mat.HasProperty("_SrcBlend"))
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);

                if (mat.HasProperty("_DstBlend"))
                    mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                if (mat.HasProperty("_ZWrite"))
                    mat.SetFloat("_ZWrite", 0);

                mat.renderQueue = 3000;
            }
        }

        private void DisableColliders()
        {
            var colliders = _preview.GetComponentsInChildren<Collider>();
            
            foreach (var collider in colliders)
                collider.enabled = false;
        }

        public void UpdatePreview(Vector3 position, bool isOccupied)
        {
            if (_preview == null)
                return;

            position.y += _config.PreviewYOffset;
            _preview.transform.position = position;
            UpdateMaterial(isOccupied);
        }

        public void UpdatePreview(Vector3 position, bool isOccupied, float rotation)
        {
            if (_preview == null)
                return;

            position.y += _config.PreviewYOffset;
            _preview.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, rotation, 0f));
            UpdateMaterial(isOccupied);
        }

        public void UpdatePreview(Vector3 position, bool isOccupied, bool hasGroundSupport)
        {
            if (_preview == null)
                return;

            position.y += _config.PreviewYOffset;
            _preview.transform.position = position;
            UpdateMaterial(isOccupied || !hasGroundSupport);
        }

        public void UpdatePreview(float rotation)
        {
            if (_preview == null)
                return;

            _preview.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        public void SetCanAfford(bool canAfford)
        {
            _canAfford = canAfford;
        }

        public void Cancel()
        {
            if (_preview != null)
                DestroyPreview();
        }

        private void DestroyPreview()
        {
            Object.Destroy(_preview.gameObject);
            _preview = null;
            _currentPreviewId = null;
            _previewMaterials.Clear();
        }

        private void UpdateMaterial(bool isOccupied)
        {
            if (_preview == null)
                return;

            Color color;

            if (isOccupied)
                color = Color.red;
            else if (!_canAfford)
                color = Color.red;
            else
                color = Color.green;

            color.a = _config.PreviewAlpha;

            foreach (var mat in _previewMaterials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
            }
        }
    }
}