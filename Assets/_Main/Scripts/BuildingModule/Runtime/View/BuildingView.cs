using System;
using System.Collections.Generic;
using BaseModule;
using ComponentsModule;
using InventoryModule;
using UnityEngine;
using UnityEngine.Rendering;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId, IEntity, IInteractionGate, IHoldInteractable, IInteractionGateBypass
    {
        [SerializeField] private Collider collisionCollider;

        private string _buildingConfigId;
        private string _instanceId;
        private string _repairDisplayName;
        private BuildingModel _model;
        private Material[] _materials;
        private Func<bool> _canRepair;
        private Func<float> _repairDuration;
        private Action _repair;

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
    }
}
