using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingMenuConfig", menuName = "Game/Configs/Constructing/BuildingMenuConfig")]
    public class BuildingMenuConfig : ScriptableObject
    {
        [Header("Animation")]
        [SerializeField] private float animationOffset = 150f;

        [Header("Layout")]
        [SerializeField] private float itemSpacing = 150f;

        [Header("Item")]
        [SerializeField] private float itemSize = 100f;

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.3f;

        [Header("Visual")]
        [SerializeField] private float minItemScale = 0.6f;
        [SerializeField] private float minItemAlpha = 0.3f;

        public float AnimationOffset => animationOffset;
        public float ItemSpacing => itemSpacing;
        public float ItemSize => itemSize;
        public float SmoothTime => smoothTime;
        public float MinItemScale => minItemScale;
        public float MinItemAlpha => minItemAlpha;
    }
}
