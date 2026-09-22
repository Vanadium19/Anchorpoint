using UnityEngine;
using InventoryModule;
using BaseModule;
using ComponentsModule;
using System;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId, IInteractable
    {
        [SerializeField] private Collider collisionCollider;
        [SerializeField] private GameObject initialVisual;
        [SerializeField] private Transform upgradeVisualContainer;

        private string _buildingConfigId;
        private string _instanceId;
        private string _displayName;
        private GameObject _upgradeVisual;
        private bool _isUpgradeAvailable;

        public Collider CollisionCollider => collisionCollider;
        public IExternalUI ExternalUI => GetComponent<IExternalUI>();
        public string DisplayName => _displayName;
        public string HintKey => InteractionHintKeys.Upgrade;

        public event Action<BuildingView> UpgradeRequested;

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

        public bool CanInteract(Transform interactor) => _isUpgradeAvailable;

        public void Interact(Transform interactor) => UpgradeRequested?.Invoke(this);

        public void RenderUpgradeInteraction(bool isAvailable, string displayName)
        {
            _isUpgradeAvailable = isAvailable;
            _displayName = displayName;
        }

        public void RenderUpgradeVisual(GameObject visualPrefab)
        {
            if (initialVisual != null)
                initialVisual.SetActive(visualPrefab == null);

            if (_upgradeVisual != null)
                Destroy(_upgradeVisual);

            if (visualPrefab == null || upgradeVisualContainer == null)
                return;

            _upgradeVisual = Instantiate(visualPrefab, upgradeVisualContainer);
            _upgradeVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _upgradeVisual.transform.localScale = Vector3.one;
        }

        public MeshRenderer[] GetAllMeshRenderers()
        {
            return GetComponentsInChildren<MeshRenderer>();
        }

        public MeshRenderer GetMainMeshRenderer()
        {
            return GetComponentInChildren<MeshRenderer>();
        }
    }
}
