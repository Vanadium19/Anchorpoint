using System;
using System.Collections.Generic;
using BaseModule;
using ComponentsModule;
using InventoryModule;
using UnityEngine;
using UnityEngine.Rendering;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId, IEntity, IInteractionGate
    {
        [SerializeField] private Collider collisionCollider;

        private string _buildingConfigId;
        private string _instanceId;
        private BuildingModel _model;
        private Material[] _materials;

        public Collider CollisionCollider => collisionCollider;
        public IExternalUI ExternalUI => GetComponent<IExternalUI>();
        public bool CanInteract => _model == null || _model.State == BuildingState.Active;

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

        public void Initialize(BuildingModel model)
        {
            _model = model;
            CollectMaterials();
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
