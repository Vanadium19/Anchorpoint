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

        [Header("Lifecycle")]
        [SerializeField] private float constructionTime = 5f;
        [SerializeField] [Min(1f)] private float maxHealth = 100f;
        [SerializeField, Range(0f, 1f)] private float brokenAlpha = 0.45f;

        [Header("Repair")]
        [SerializeField] private Price repairPrice;
        [SerializeField] private float repairTime = 3f;

        [Header("Repair Support")]
        [SerializeField, Range(0f, 100f)] private float repairReductionPercent;

        [Header("Scout Tower")]
        [SerializeField] private bool isScoutTower;
        [SerializeField] [Min(0f)] private float raidPreparationSeconds = 60f;
        [SerializeField] private string raidWarningLocalizationKey = "raid_scout_warning";
        [SerializeField] [Min(0f)] private float raidWarningDisplaySeconds = 6f;

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
        public float ConstructionTime => constructionTime;
        public float MaxHealth => maxHealth;
        public float BrokenAlpha => brokenAlpha;
        public Price RepairPrice => repairPrice;
        public float RepairTime => repairTime;
        public float RepairReductionPercent => repairReductionPercent;
        public bool IsScoutTower => isScoutTower;
        public float RaidPreparationSeconds => raidPreparationSeconds;
        public string RaidWarningLocalizationKey => raidWarningLocalizationKey;
        public float RaidWarningDisplaySeconds => raidWarningDisplaySeconds;
        public LayerMask AllowedBuildLayers => allowedBuildLayers;
        public int BasePoints => basePoints;
    }
}
