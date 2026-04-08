using System.Collections.Generic;
using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingCategoryConfig", menuName = "Game/Configs/Constructing/CategoryConfig")]
    public class BuildingCategoryConfig : ScriptableObject
    {
        [SerializeField] private BuildingCategory category;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private int order;

        public BuildingCategory Category => category;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int Order => order;
    }
}
