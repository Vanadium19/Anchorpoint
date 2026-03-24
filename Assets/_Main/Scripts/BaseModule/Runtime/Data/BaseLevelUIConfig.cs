using UnityEngine;

namespace BaseModule
{
    [CreateAssetMenu(fileName = "BaseLevelUIConfig", menuName = "Game/Configs/BaseLevelUIConfig")]
    public class BaseLevelUIConfig : ScriptableObject
    {
        [Header("Text Formats")]
        [SerializeField] private string levelFormat = "LVL {0}";
        [SerializeField] private string levelUpFormat = "LVL {0} (+{1})";
        [SerializeField] private string pointsFormat = "{0}/{1}";
        [SerializeField] private string pointsNoThresholdFormat = "{0}";
        [SerializeField] private string previewPointsFormat = "+{0}";

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

        public string LevelFormat => levelFormat;
        public string LevelUpFormat => levelUpFormat;
        public string PointsFormat => pointsFormat;
        public string PointsNoThresholdFormat => pointsNoThresholdFormat;
        public string PreviewPointsFormat => previewPointsFormat;
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
