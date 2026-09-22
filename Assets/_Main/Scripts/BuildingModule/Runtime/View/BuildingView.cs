using System.Collections.Generic;
using BaseModule;
using InventoryModule;
using UnityEngine;
using UnityEngine.Rendering;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly Color AvailableUpgradeColor = Color.green;
        private static readonly Color UnavailableUpgradeColor = Color.red;

        [SerializeField] private Collider collisionCollider;
        [SerializeField] private GameObject initialVisual;
        [SerializeField] private Transform upgradeVisualContainer;

        private string _buildingConfigId;
        private string _instanceId;
        private GameObject _upgradeVisual;
        private GameObject _upgradePreviewVisual;
        private BuildingUpgradeHighlightState _upgradeHighlightState;
        private GameObject _highlightVisualPrefab;
        private float _highlightAlpha;

        private readonly List<MeshRenderer> _highlightRenderers = new();
        private readonly List<Material[]> _originalMaterials = new();
        private readonly List<Material> _highlightMaterials = new();
        private readonly List<ShadowCastingMode> _originalShadowModes = new();
        private readonly List<bool> _originalReceiveShadows = new();

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

        private void OnDestroy() => ClearUpgradeHighlight();

        public void RenderUpgradeVisual(GameObject visualPrefab)
        {
            ClearUpgradeHighlight();

            if (initialVisual != null)
                initialVisual.SetActive(visualPrefab == null);

            if (_upgradeVisual != null)
            {
                _upgradeVisual.SetActive(false);
                Destroy(_upgradeVisual);
            }

            _upgradeVisual = null;

            if (visualPrefab != null && upgradeVisualContainer != null)
            {
                _upgradeVisual = Instantiate(visualPrefab, upgradeVisualContainer);
                _upgradeVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _upgradeVisual.transform.localScale = Vector3.one;
            }

            ApplyUpgradeHighlight();
        }

        public void SetUpgradeHighlight(BuildingUpgradeHighlightState state, float alpha, GameObject visualPrefab = null)
        {
            if (_upgradeHighlightState == state
                && _highlightVisualPrefab == visualPrefab
                && Mathf.Approximately(_highlightAlpha, alpha))
                return;

            ClearUpgradeHighlight();
            _upgradeHighlightState = state;
            _highlightVisualPrefab = visualPrefab;
            _highlightAlpha = alpha;
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
            if (_upgradeHighlightState == BuildingUpgradeHighlightState.None
                || _highlightVisualPrefab == null
                || upgradeVisualContainer == null)
                return;

            CreateUpgradePreview();

            foreach (var meshRenderer in _upgradePreviewVisual.GetComponentsInChildren<MeshRenderer>())
            {
                var originalMaterials = meshRenderer.sharedMaterials;
                var transparentMaterials = new Material[originalMaterials.Length];

                for (var index = 0; index < originalMaterials.Length; index++)
                {
                    if (originalMaterials[index] == null)
                        continue;

                    var material = new Material(originalMaterials[index]);
                    SetTransparent(material);
                    SetHighlightColor(material);
                    transparentMaterials[index] = material;
                    _highlightMaterials.Add(material);
                }

                _highlightRenderers.Add(meshRenderer);
                _originalMaterials.Add(originalMaterials);
                _originalShadowModes.Add(meshRenderer.shadowCastingMode);
                _originalReceiveShadows.Add(meshRenderer.receiveShadows);
                meshRenderer.sharedMaterials = transparentMaterials;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
            }
        }

        private void ClearUpgradeHighlight()
        {
            for (var index = 0; index < _highlightRenderers.Count; index++)
            {
                var meshRenderer = _highlightRenderers[index];

                if (meshRenderer != null)
                {
                    meshRenderer.sharedMaterials = _originalMaterials[index];
                    meshRenderer.shadowCastingMode = _originalShadowModes[index];
                    meshRenderer.receiveShadows = _originalReceiveShadows[index];
                }
            }

            foreach (var material in _highlightMaterials)
                Destroy(material);

            DestroyUpgradePreview();
            _highlightRenderers.Clear();
            _originalMaterials.Clear();
            _highlightMaterials.Clear();
            _originalShadowModes.Clear();
            _originalReceiveShadows.Clear();
        }

        private void CreateUpgradePreview()
        {
            SetCurrentVisualActive(false);
            _upgradePreviewVisual = Instantiate(_highlightVisualPrefab, upgradeVisualContainer);
            _upgradePreviewVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _upgradePreviewVisual.transform.localScale = Vector3.one;

            foreach (var collider in _upgradePreviewVisual.GetComponentsInChildren<Collider>())
                collider.enabled = false;
        }

        private void DestroyUpgradePreview()
        {
            if (_upgradePreviewVisual != null)
            {
                _upgradePreviewVisual.SetActive(false);
                Destroy(_upgradePreviewVisual);
                _upgradePreviewVisual = null;
            }

            SetCurrentVisualActive(true);
        }

        private void SetCurrentVisualActive(bool isActive)
        {
            if (initialVisual != null)
                initialVisual.SetActive(isActive && _upgradeVisual == null);

            if (_upgradeVisual != null)
                _upgradeVisual.SetActive(isActive);
        }

        private Color GetUpgradeHighlightColor()
        {
            return _upgradeHighlightState == BuildingUpgradeHighlightState.Available
                ? AvailableUpgradeColor
                : UnavailableUpgradeColor;
        }

        private static void SetTransparent(Material material)
        {
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);

            if (material.HasProperty("_SrcBlend"))
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);

            if (material.HasProperty("_DstBlend"))
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);

            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.renderQueue = 3000;
        }

        private void SetHighlightColor(Material material)
        {
            var color = GetUpgradeHighlightColor();
            color.a = _highlightAlpha;

            if (material.HasProperty(BaseColorId))
                material.SetColor(BaseColorId, color);
            else if (material.HasProperty(ColorId))
                material.SetColor(ColorId, color);
        }
    }
}
