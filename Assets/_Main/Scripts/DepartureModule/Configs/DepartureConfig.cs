using UnityEngine;

namespace DepartureModule
{
    [CreateAssetMenu(fileName = "DepartureConfig", menuName = "Configs/Departure/DepartureConfig")]
    public class DepartureConfig : ScriptableObject
    {
        public float DepartureTime = 3.0f;
        public string TargetSceneName = "MainLevel";
        public string TimerTextFormat = "ОТПРАВЛЕНИЕ ЧЕРЕЗ: {0:F1}";
    }
}