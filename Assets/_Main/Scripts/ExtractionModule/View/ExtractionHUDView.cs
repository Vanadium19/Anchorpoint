using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ExtractionModule
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
            timerContainer?.SetActive(false);
            successPanel?.SetActive(false);

            if (restartButton)
                restartButton.onClick.AddListener(OnRestartClicked);
        }

        public void ShowTimer(float timeRemaining, string textFormat)
        {
            timerContainer?.SetActive(true);
            timerText.text = string.Format(textFormat, timeRemaining);
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

        private void OnRestartClicked() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}