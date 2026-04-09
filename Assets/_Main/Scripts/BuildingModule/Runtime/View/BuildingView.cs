using UnityEngine;
using InventoryModule;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour
    {
        [SerializeField] private Collider collisionCollider;

        public Collider CollisionCollider => collisionCollider;
        public IContainerUI ContainerUI => GetComponent<IContainerUI>();

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