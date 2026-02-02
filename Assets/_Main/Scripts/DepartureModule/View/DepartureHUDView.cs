using UnityEngine;
using TMPro;

namespace DepartureModule
{
    public class DepartureHUDView : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        [SerializeField] private TMP_Text timerText;

        public void ShowTimer(float time, string format)
        {
            container.SetActive(true);
            timerText.text = string.Format(format, time);
        }

        public void Hide()
        {
            if (this == null || container == null) return;
            container.SetActive(false);
        }
    }
}