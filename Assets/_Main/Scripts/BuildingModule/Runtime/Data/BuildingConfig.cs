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

        [Header("Defense")]
        [SerializeField] private bool isDefensive;
        [SerializeField] [Min(0.1f)] private float defenseSearchRadius = 10f;
        [SerializeField] [Min(0f)] private float defenseDamage = 10f;
        [SerializeField] [Min(0.01f)] private float defenseAttackRate = 1f;

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
        public bool IsDefensive => isDefensive;
        public float DefenseSearchRadius => defenseSearchRadius;
        public float DefenseDamage => defenseDamage;
        public float DefenseAttackRate => defenseAttackRate;
        public LayerMask AllowedBuildLayers => allowedBuildLayers;
        public int BasePoints => basePoints;
    }
}
