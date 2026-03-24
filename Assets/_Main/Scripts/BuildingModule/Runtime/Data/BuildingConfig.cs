using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "Game/Configs/Constructing/BuildingConfig")]
    public class BuildingConfig : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private BuildingCategory category;
        [SerializeField] private BuildingView prefab;
        [SerializeField] private Price price;
        [SerializeField] private Sprite icon;
        [SerializeField] private int order;
        [Header("Placement Restrictions")]
        [SerializeField] private LayerMask allowedBuildLayers = ~0;

        [Header("Base Level")]
        [SerializeField] private int basePoints;

        public string Id => id;
        public BuildingCategory Category => category;
        public BuildingView Prefab => prefab;
        public Price Price => price;
        public Sprite Icon => icon;
        public int Order => order;
        public LayerMask AllowedBuildLayers => allowedBuildLayers;
        public int BasePoints => basePoints;
    }
}