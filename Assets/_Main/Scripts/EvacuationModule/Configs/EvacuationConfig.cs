using UnityEngine;

namespace EvacuationModule
{
    [CreateAssetMenu(fileName = "EvacuationConfig", menuName = "Configs/Evacuation/EvacuationConfig")]
    public class EvacuationConfig : ScriptableObject
    {
        public float Duration = 5.0f;
        public string TimerTextFormat = "EVAC IN: {0:F1}";

        public EvacuationCompleteMode CompleteMode = EvacuationCompleteMode.ShowSuccessScreen;

        public string TargetSceneName = "MainLevel";
        public string BaseSceneName = "BaseScene";
    }
}