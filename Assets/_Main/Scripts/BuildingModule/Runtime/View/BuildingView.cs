using System.Collections.Generic;
using BaseModule;
using InventoryModule;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly Color AvailableUpgradeColor = new(0f, 1f, 1f, 1f);
        private static readonly Color UnavailableUpgradeColor = new(1f, 0.2f, 0.2f, 1f);

        [SerializeField] private Collider collisionCollider;
        [SerializeField] private GameObject initialVisual;
        [SerializeField] private Transform upgradeVisualContainer;

        private string _buildingConfigId;
        private string _instanceId;
        private GameObject _upgradeVisual;
        private BuildingUpgradeHighlightState _upgradeHighlightState;

        private readonly List<MeshRenderer> _highlightRenderers = new();
        private readonly List<MaterialPropertyBlock> _originalPropertyBlocks = new();

        public Collider CollisionCollider => collisionCollider;
        public IExternalUI ExternalUI => GetComponent<IExternalUI>();

        public string BuildingConfigId
        {
            get => _buildingConfigId;
            set => _buildingConfigId = value;
        }

        public string InstanceId
        {
            get => _instanceId;
            set => _instanceId = value;
        }

        public void RenderUpgradeVisual(GameObject visualPrefab)
        {
            ClearUpgradeHighlight();

            if (initialVisual != null)
                initialVisual.SetActive(visualPrefab == null);

            if (_upgradeVisual != null)
                Destroy(_upgradeVisual);

            if (visualPrefab != null && upgradeVisualContainer != null)
            {
                _upgradeVisual = Instantiate(visualPrefab, upgradeVisualContainer);
                _upgradeVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _upgradeVisual.transform.localScale = Vector3.one;
            }

            ApplyUpgradeHighlight();
        }

        public void SetUpgradeHighlight(BuildingUpgradeHighlightState state)
        {
            if (_upgradeHighlightState == state)
                return;

            ClearUpgradeHighlight();
            _upgradeHighlightState = state;
            ApplyUpgradeHighlight();
        }

        public MeshRenderer[] GetAllMeshRenderers()
        {
            return GetComponentsInChildren<MeshRenderer>();
        }

        public MeshRenderer GetMainMeshRenderer()
        {
            return GetComponentInChildren<MeshRenderer>();
        }

        private void ApplyUpgradeHighlight()
        {
            if (_upgradeHighlightState == BuildingUpgradeHighlightState.None)
                return;

            foreach (var meshRenderer in GetAllMeshRenderers())
            {
                if (!TryGetColorProperty(meshRenderer, out var colorProperty))
                    continue;

                var originalPropertyBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(originalPropertyBlock);
                _highlightRenderers.Add(meshRenderer);
                _originalPropertyBlocks.Add(originalPropertyBlock);

                var highlightPropertyBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(highlightPropertyBlock);
                highlightPropertyBlock.SetColor(colorProperty, GetUpgradeHighlightColor());
                meshRenderer.SetPropertyBlock(highlightPropertyBlock);
            }
        }

        private void ClearUpgradeHighlight()
        {
            for (var index = 0; index < _highlightRenderers.Count; index++)
            {
                var meshRenderer = _highlightRenderers[index];

                if (meshRenderer != null)
                    meshRenderer.SetPropertyBlock(_originalPropertyBlocks[index]);
            }

            _highlightRenderers.Clear();
            _originalPropertyBlocks.Clear();
        }

        private Color GetUpgradeHighlightColor()
        {
            return _upgradeHighlightState == BuildingUpgradeHighlightState.Available
                ? AvailableUpgradeColor
                : UnavailableUpgradeColor;
        }

        private static bool TryGetColorProperty(MeshRenderer meshRenderer, out int colorProperty)
        {
            colorProperty = 0;
            var material = meshRenderer.sharedMaterial;

            if (material == null)
                return false;

            if (material.HasProperty(BaseColorId))
            {
                colorProperty = BaseColorId;
                return true;
            }

            if (!material.HasProperty(ColorId))
                return false;

            colorProperty = ColorId;
            return true;
        }
    }
}
