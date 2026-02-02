using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "Game/Configs/Constructing/BuildingConfig")]
    public class BuildingConfig : ScriptableObject
    {
        [SerializeField] private BuildingName name;
        [SerializeField] private BuildingView prefab;
        [SerializeField] private Price price;

        public BuildingName Name => name;
        public BuildingView Prefab => prefab;
        public Price Price => price;
    }
}