using UnityEngine;
using UtilsModule;

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

        [Header("Localization")]
        [SerializeField] private string nameKey = "";
        [Header("Placement Restrictions")]
        [SerializeField] private LayerMask allowedBuildLayers = ~0;

        [Header("Base Level")]
        [SerializeField] private int basePoints;

        public string Id => id;
        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? id : LocalizedText.Get(nameKey);
        public BuildingCategory Category => category;
        public BuildingView Prefab => prefab;
        public Price Price => price;
        public Sprite Icon => icon;
        public int Order => order;
        public LayerMask AllowedBuildLayers => allowedBuildLayers;
        public int BasePoints => basePoints;
    }
}