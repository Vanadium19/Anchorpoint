using TMPro;
using UnityEngine;

namespace EvacuationModule
{
    public class EvacuationView : MonoBehaviour
    {
        [SerializeField] private GameObject timerContainer;
        [SerializeField] private TMP_Text timerText;

        [Tooltip("Can be null")]
        [SerializeField] private GameObject successPanel;

        private void Awake()
        {
            timerContainer?.SetActive(false);

            if (successPanel)
                successPanel.SetActive(false);
        }

        public void ShowTimer(string time)
        {
            timerContainer?.SetActive(true);
            timerText.text = time;
        }

        public void HideTimer() => timerContainer?.SetActive(false);

        public void ShowSuccessScreen()
        {
            if (!successPanel)
                return;

            successPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}