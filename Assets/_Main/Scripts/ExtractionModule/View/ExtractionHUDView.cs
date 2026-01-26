using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ExtractionModule.View
{
    public class ExtractionHUDView : MonoBehaviour
    {
        [Header("Timer UI")]
        [SerializeField] private GameObject timerContainer;
        [SerializeField] private TMP_Text timerText;

        [Header("Success UI")]
        [SerializeField] private GameObject successPanel;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (timerContainer) timerContainer.SetActive(false);
            if (successPanel) successPanel.SetActive(false);

            if (restartButton)
                restartButton.onClick.AddListener(OnRestartClicked);
        }

        public void ShowTimer(float timeRemaining, string textFormat)
        {
            if (this == null || timerContainer == null) return;
            timerContainer.SetActive(true);
            if (timerText)
                timerText.text = string.Format(textFormat, timeRemaining);
        }

        public void HideTimer()
        {
            if (this == null || timerContainer == null) return;
            timerContainer.SetActive(false);
        }

        public void ShowSuccessScreen()
        {
            if (successPanel == null) return;
            successPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnRestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}