using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingPriceUIConfig", menuName = "Game/Configs/BuildingPriceUIConfig")]
    public class BuildingPriceUIConfig : ScriptableObject
    {
        [Header("Layout")]
        [SerializeField] private float itemHeight = 50f;

        public float ItemHeight => itemHeight;
    }
}
