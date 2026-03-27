using UnityEngine;

namespace BaseModule
{
    [CreateAssetMenu(fileName = "BaseLevelConfig", menuName = "Game/Configs/BaseLevelConfig")]
    public class BaseLevelConfig : ScriptableObject
    {
        [SerializeField] private int[] levelThresholds = System.Array.Empty<int>();

        public int[] LevelThresholds => levelThresholds;
    }
}
