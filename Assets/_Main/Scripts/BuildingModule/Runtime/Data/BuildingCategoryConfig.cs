using System.Collections.Generic;
using UnityEngine;
using UtilsModule;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingCategoryConfig", menuName = "Game/Configs/Constructing/CategoryConfig")]
    public class BuildingCategoryConfig : ScriptableObject
    {
        [SerializeField] private BuildingCategory category;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private int order;

        [Header("Localization")]
        [SerializeField] private string nameKey = "";

        public BuildingCategory Category => category;
        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);
        public Sprite Icon => icon;
        public int Order => order;
    }
}
