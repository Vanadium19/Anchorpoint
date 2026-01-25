using UnityEngine;
using UnityEngine.UI;
using System;

namespace UIModule
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button restartButton;

        public event Action RestartClicked;

        private void OnEnable() => restartButton.onClick.AddListener(OnRestartButtonClicked);

        private void OnDisable() => restartButton.onClick.RemoveListener(OnRestartButtonClicked);

        public void Show()
        {
            panel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide() => panel.SetActive(false);

        private void OnRestartButtonClicked() => RestartClicked?.Invoke();
    }
}