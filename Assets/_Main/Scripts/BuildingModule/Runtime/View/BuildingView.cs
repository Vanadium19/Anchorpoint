using System;
using System.Collections.Generic;
using BaseModule;
using ComponentsModule;
using DG.Tweening;
using InventoryModule;
using UnityEngine;
using UnityEngine.Rendering;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId, IEntity, IInteractionGate, IHoldInteractable, IInteractionGateBypass
    {
        private const float HighlightTintStrength = 0.35f;
        private const float UpgradePunchScale = 0.12f;
        private const float UpgradePunchDuration = 0.45f;
        private const int UpgradePunchVibrato = 6;
        private const float UpgradePunchElasticity = 0.6f;

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
        private GameObject _previewVisualPrefab;
        private BuildingUpgradeHighlightState _previewState;
        private BuildingUpgradeHighlightState _highlightState;
        private MaterialPropertyBlock _propertyBlock;
        private Tween _upgradeTween;
        private string _repairDisplayName;
        private BuildingModel _model;
        private Material[] _materials;
        private Func<bool> _canRepair;
        private Func<float> _repairDuration;
        private Action _repair;
        private BuildingState _visualState;
        private float _brokenAlpha = 1f;

        private readonly List<MeshRenderer> _tintedRenderers = new();
        private readonly List<Material> _previewMaterials = new();

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

        string IInteractable.DisplayName => _repairDisplayName;
        string IInteractable.HintKey => InteractionHintKeys.Repair;
        float IHoldInteractable.HoldDuration => Mathf.Max(_repairDuration?.Invoke() ?? 0f, 0f);
        bool IInteractionGate.CanInteract => _model == null || _model.State == BuildingState.Active;
        bool IInteractionGateBypass.CanBypassInteractionGate => _model != null && _model.State == BuildingState.Broken;

        private void OnDestroy()
        {
            _upgradeTween?.Kill();
            ClearUpgradePreview();
        }

        public void RenderUpgradeVisual(GameObject visualPrefab)
        {
            _upgradeTween?.Kill(true);
            ClearHighlightTint();
            ClearUpgradePreview();
            _highlightState = BuildingUpgradeHighlightState.None;

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

            CollectMaterials();
            SetStateVisual(_visualState, _brokenAlpha);
        }

        public void RenderUpgradeHighlight(BuildingUpgradeHighlightState state)
        {
            if (_highlightState == state)
                return;

            ClearHighlightTint();
            _highlightState = state;
            ApplyHighlightTint();
        }

        public void RenderUpgradePreview(GameObject visualPrefab, BuildingUpgradeHighlightState state, float alpha)
        {
            if (visualPrefab == null
                || upgradeVisualContainer == null
                || state == BuildingUpgradeHighlightState.None
                || alpha <= 0f)
            {
                ClearUpgradePreview();
                return;
            }

            if (_upgradePreviewVisual == null || _previewVisualPrefab != visualPrefab || _previewState != state)
            {
                ClearUpgradePreview();
                _previewVisualPrefab = visualPrefab;
                _previewState = state;
                CreateUpgradePreview();
            }

            foreach (var material in _previewMaterials)
                SetMaterialColor(material, GetHighlightColor(_previewState), alpha);
        }

        public void PlayUpgradeAnimation()
        {
            var currentVisual = GetCurrentVisual();

            if (currentVisual == null)
                return;

            _upgradeTween?.Kill(true);
            _upgradeTween = currentVisual.transform
                .DOPunchScale(Vector3.one * UpgradePunchScale, UpgradePunchDuration, UpgradePunchVibrato, UpgradePunchElasticity)
                .SetLink(currentVisual);
        }

        public void Initialize(BuildingModel model)
        {
            _model = model;
            CollectMaterials();
        }

        public void ConfigureRepairInteraction(
            string displayName,
            Func<float> repairDuration,
            Func<bool> canRepair,
            Action repair)
        {
            _repairDisplayName = displayName;
            _repairDuration = repairDuration;
            _canRepair = canRepair;
            _repair = repair;
        }

        public T Get<T>() where T : class
        {
            if (TryGet<T>(out var value))
                return value;

            throw new InvalidOperationException($"{typeof(T).Name} is not available on {name}");
        }

        public bool TryGet<T>(out T value) where T : class
        {
            value = _model as T;
            return value != null;
        }

        public MeshRenderer[] GetAllMeshRenderers() => GetComponentsInChildren<MeshRenderer>();

        public MeshRenderer GetMainMeshRenderer() => GetComponentInChildren<MeshRenderer>();

        public void SetStateVisual(BuildingState state, float brokenAlpha)
        {
            _visualState = state;
            _brokenAlpha = brokenAlpha;
            var isBroken = state == BuildingState.Broken;
            SetTransparent(isBroken);
            SetAlpha(isBroken ? brokenAlpha : 1f);
        }

        bool IInteractable.CanInteract(Transform interactor) => _canRepair?.Invoke() ?? false;

        void IInteractable.Interact(Transform interactor) => _repair?.Invoke();

        private void CollectMaterials()
        {
            var materials = new List<Material>();

            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                foreach (var material in meshRenderer.materials)
                    materials.Add(material);
            }

            _materials = materials.ToArray();
        }

        private void SetTransparent(bool isTransparent)
        {
            if (_materials == null)
                return;

            foreach (var material in _materials)
            {
                if (material == null)
                    continue;

                if (material.HasProperty("_Surface"))
                    material.SetFloat("_Surface", isTransparent ? 1f : 0f);

                if (material.HasProperty("_SrcBlend"))
                    material.SetFloat("_SrcBlend", isTransparent ? (float)BlendMode.SrcAlpha : (float)BlendMode.One);

                if (material.HasProperty("_DstBlend"))
                    material.SetFloat("_DstBlend", isTransparent ? (float)BlendMode.OneMinusSrcAlpha : (float)BlendMode.Zero);

                if (material.HasProperty("_ZWrite"))
                    material.SetFloat("_ZWrite", isTransparent ? 0f : 1f);

                material.renderQueue = isTransparent ? 3000 : -1;
            }
        }

        private void SetAlpha(float alpha)
        {
            if (_materials == null)
                return;

            foreach (var material in _materials)
            {
                if (material == null)
                    continue;

                if (material.HasProperty("_BaseColor"))
                {
                    var color = material.GetColor("_BaseColor");
                    color.a = alpha;
                    material.SetColor("_BaseColor", color);
                    continue;
                }

                if (material.HasProperty("_Color"))
                {
                    var color = material.GetColor("_Color");
                    color.a = alpha;
                    material.SetColor("_Color", color);
                }
            }
        }

        private void ApplyHighlightTint()
        {
            var currentVisual = GetCurrentVisual();

            if (_highlightState == BuildingUpgradeHighlightState.None || currentVisual == null)
                return;

            _propertyBlock ??= new MaterialPropertyBlock();
            var highlightColor = GetHighlightColor(_highlightState);

            foreach (var meshRenderer in currentVisual.GetComponentsInChildren<MeshRenderer>())
            {
                var materials = meshRenderer.sharedMaterials;

                for (var index = 0; index < materials.Length; index++)
                {
                    if (!TryGetColorId(materials[index], out var colorId))
                        continue;

                    var tintedColor = Color.Lerp(materials[index].GetColor(colorId), highlightColor, HighlightTintStrength);
                    _propertyBlock.Clear();
                    _propertyBlock.SetColor(colorId, tintedColor);
                    meshRenderer.SetPropertyBlock(_propertyBlock, index);
                }

                _tintedRenderers.Add(meshRenderer);
            }
        }

        private void ClearHighlightTint()
        {
            if (_tintedRenderers.Count == 0)
                return;

            _propertyBlock.Clear();

            foreach (var meshRenderer in _tintedRenderers)
            {
                if (meshRenderer == null)
                    continue;

                for (var index = 0; index < meshRenderer.sharedMaterials.Length; index++)
                    meshRenderer.SetPropertyBlock(_propertyBlock, index);
            }

            _tintedRenderers.Clear();
        }

        private void CreateUpgradePreview()
        {
            SetCurrentVisualActive(false);
            _upgradePreviewVisual = Instantiate(_previewVisualPrefab, upgradeVisualContainer);
            _upgradePreviewVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _upgradePreviewVisual.transform.localScale = Vector3.one;

            foreach (var collider in _upgradePreviewVisual.GetComponentsInChildren<Collider>())
                collider.enabled = false;

            foreach (var meshRenderer in _upgradePreviewVisual.GetComponentsInChildren<MeshRenderer>())
            {
                var materials = meshRenderer.sharedMaterials;

                for (var index = 0; index < materials.Length; index++)
                {
                    if (materials[index] == null)
                        continue;

                    materials[index] = new Material(materials[index]);
                    SetTransparent(materials[index]);
                    _previewMaterials.Add(materials[index]);
                }

                meshRenderer.sharedMaterials = materials;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
            }
        }

        private void ClearUpgradePreview()
        {
            foreach (var material in _previewMaterials)
                Destroy(material);

            _previewMaterials.Clear();
            _previewVisualPrefab = null;
            _previewState = BuildingUpgradeHighlightState.None;

            if (_upgradePreviewVisual == null)
                return;

            _upgradePreviewVisual.SetActive(false);
            Destroy(_upgradePreviewVisual);
            _upgradePreviewVisual = null;
            SetCurrentVisualActive(true);
        }

        private GameObject GetCurrentVisual() => _upgradeVisual != null ? _upgradeVisual : initialVisual;

        private void SetCurrentVisualActive(bool isActive)
        {
            var currentVisual = GetCurrentVisual();

            if (currentVisual != null)
                currentVisual.SetActive(isActive);
        }

        private static Color GetHighlightColor(BuildingUpgradeHighlightState state)
        {
            return state == BuildingUpgradeHighlightState.Available
                ? AvailableUpgradeColor
                : UnavailableUpgradeColor;
        }

        private static bool TryGetColorId(Material material, out int colorId)
        {
            colorId = BaseColorId;

            if (material == null)
                return false;

            if (material.HasProperty(BaseColorId))
                return true;

            colorId = ColorId;
            return material.HasProperty(ColorId);
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

        private static void SetMaterialColor(Material material, Color color, float alpha)
        {
            if (!TryGetColorId(material, out var colorId))
                return;

            color.a = alpha;
            material.SetColor(colorId, color);
        }
    }
}
