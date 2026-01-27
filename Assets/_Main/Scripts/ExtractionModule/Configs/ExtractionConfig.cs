using UnityEngine;

namespace ExtractionModule
{
    [CreateAssetMenu(fileName = "ExtractionConfig", menuName = "Configs/Extraction")]
    //FIXME: Tooltip на английском
    public class ExtractionConfig : ScriptableObject
    {
        [Tooltip("Время в секундах, которое нужно простоять в зоне")]
        public float ExtractionTime = 5.0f;

        [Tooltip("Текст таймера. {0} заменится на время.")]
        public string TimerTextFormat = "EVAC IN: {0:F1}";
    }
}