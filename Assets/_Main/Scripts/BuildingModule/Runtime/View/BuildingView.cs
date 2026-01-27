using UnityEngine;

namespace BuildingModule
{
    public class BuildingView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        private BuildingName _name;

        public MeshRenderer MeshRenderer => meshRenderer;
        public BuildingName Name => _name;

        public void SetBuildingName(BuildingName value) => _name = value;
    }
}