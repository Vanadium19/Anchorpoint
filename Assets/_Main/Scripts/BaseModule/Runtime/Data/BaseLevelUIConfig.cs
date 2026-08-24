using UnityEngine;

namespace BaseModule
{
    [CreateAssetMenu(fileName = "BaseLevelUIConfig", menuName = "Game/Configs/BaseLevelUIConfig")]
    public class BaseLevelUIConfig : ScriptableObject
    {
        [Header("Animation Durations")]
        [SerializeField] private float progressAnimationDuration = 0.5f;
        [SerializeField] private float previewProgressAnimationDuration = 0.3f;
        [SerializeField] private float pointsCountUpDuration = 0.5f;
        [SerializeField] private float levelChangeAnimationDuration = 0.3f;
        [SerializeField] private float slideAnimationDuration = 0.3f;

        [Header("Animation Settings")]
        [SerializeField] private float levelPunchScale = 0.2f;
        [SerializeField] private int levelPunchVibrato = 1;
        [SerializeField] private float levelPunchElasticity = 0.5f;
        [SerializeField] private float slideOffset = 200f;

        public float ProgressAnimationDuration => progressAnimationDuration;
        public float PreviewProgressAnimationDuration => previewProgressAnimationDuration;
        public float PointsCountUpDuration => pointsCountUpDuration;
        public float LevelChangeAnimationDuration => levelChangeAnimationDuration;
        public float SlideAnimationDuration => slideAnimationDuration;
        public float LevelPunchScale => levelPunchScale;
        public int LevelPunchVibrato => levelPunchVibrato;
        public float LevelPunchElasticity => levelPunchElasticity;
        public float SlideOffset => slideOffset;
    }
}
