using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingPriceUIConfig", menuName = "Game/Configs/BuildingPriceUIConfig")]
    public class BuildingPriceUIConfig : ScriptableObject
    {
        [Header("Formats")]
        [SerializeField] private string headerFormat = "{0} ({1})";
        [SerializeField] private string amountFormat = "{0}/{1}";

        [Header("Layout")]
        [SerializeField] private float itemHeight = 50f;

        public string HeaderFormat => headerFormat;
        public string AmountFormat => amountFormat;
        public float ItemHeight => itemHeight;
    }
}
