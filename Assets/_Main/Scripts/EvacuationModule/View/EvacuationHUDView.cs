using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EvacuationModule
{
    public class EvacuationHUDView : MonoBehaviour
    {
        [SerializeField] private GameObject timerContainer;
        [SerializeField] private TMP_Text timerText;

        [SerializeField] private GameObject successPanel;
        [SerializeField] private Button restartButton;

        private string _baseSceneName;

        private void Awake()
        {
            if (timerContainer != null)
                timerContainer.SetActive(false);

            if (successPanel != null)
                successPanel.SetActive(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
        }

        public void Setup(string baseSceneName)
        {
            _baseSceneName = baseSceneName;
        }

        public void ShowTimer(float timeRemaining, string textFormat)
        {
            timerContainer?.SetActive(true);

            if (timerText != null)
                timerText.text = string.Format(textFormat, timeRemaining);
        }

        public void HideTimer() => timerContainer?.SetActive(false);

        public void ShowSuccessScreen()
        {
            if (successPanel == null)
                return;

            successPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnRestartClicked()
        {
            if (!string.IsNullOrEmpty(_baseSceneName))
                SceneManager.LoadScene(_baseSceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}