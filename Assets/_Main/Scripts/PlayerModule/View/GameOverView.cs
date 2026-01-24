using UnityEngine;
using UnityEngine.UI;
using System;

namespace PlayerModule.View
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button restartButton;

        public event Action RestartClicked;

        private void OnEnable()
        {
            restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
        }

        private void OnDisable()
        {
            restartButton.onClick.RemoveAllListeners();
        }

        public void Show()
        {
            panel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            panel.SetActive(false);
        }
    }
}