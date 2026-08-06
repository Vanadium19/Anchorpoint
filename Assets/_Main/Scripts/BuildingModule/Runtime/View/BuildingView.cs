using UnityEngine;
using InventoryModule;
using BaseModule;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour, IHasInstanceId
    {
        [SerializeField] private Collider collisionCollider;

        private string _buildingConfigId;

        private string _instanceId;

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