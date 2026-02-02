using UnityEngine;

namespace ExtractionModule
{
    [CreateAssetMenu(fileName = "ExtractionConfig", menuName = "Configs/Extraction")]
    public class ExtractionConfig : ScriptableObject
    {
        [Tooltip("Time for extraction")]
        public float ExtractionTime = 5.0f;

        [Tooltip("Timer text. {0} -> is time for extraction.")]
        public string TimerTextFormat = "EVAC IN: {0:F1}";

        [Header("Scene Transition")]
        public string BaseSceneName = "BaseScene";
    }
}