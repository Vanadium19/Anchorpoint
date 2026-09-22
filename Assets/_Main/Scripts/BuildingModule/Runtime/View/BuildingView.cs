using UnityEngine;
using InventoryModule;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId
    {
        [SerializeField] private Collider collisionCollider;
        [SerializeField] private GameObject initialVisual;
        [SerializeField] private Transform upgradeVisualContainer;

        private string _buildingConfigId;
        private string _instanceId;
        private GameObject _upgradeVisual;

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
