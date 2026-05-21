using UnityEngine;

namespace BaseModule
{
    [CreateAssetMenu(fileName = "BaseLevelConfig", menuName = "Game/Configs/BaseLevelConfig")]
    public class BaseLevelConfig : ScriptableObject
    {
        [SerializeField] private int[] levelThresholds = System.Array.Empty<int>();

        [SerializeField, Min(0),] private int targetVictoryLevel = 10;

        public int[] LevelThresholds => levelThresholds;
        public int TargetVictoryLevel => targetVictoryLevel;
    }
}