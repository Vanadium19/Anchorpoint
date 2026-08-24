using UnityEngine;

namespace EvacuationModule
{
    [CreateAssetMenu(fileName = "EvacuationConfig", menuName = "Configs/Evacuation/EvacuationConfig")]
    public class EvacuationConfig : ScriptableObject
    {
        public float Duration = 5.0f;

        public EvacuationCompleteMode CompleteMode = EvacuationCompleteMode.ShowSuccessScreen;

        public string TargetSceneName = "MainLevel";
        public string BaseSceneName = "BaseScene";
    }
}