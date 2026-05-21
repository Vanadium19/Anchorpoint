using System;
using UnityEngine;
using UnityEngine.UI;

namespace MenuModule
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [SerializeField] private Button startGameButton;
        [SerializeField] private Button exitButton;

        public event Action StartGameClicked;
        public event Action ExitClicked;

        private GameObject _target => panel ? panel : gameObject;

        private void OnEnable()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnDisable()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);
        }

        public void Show() => _target?.SetActive(true);

        public void Hide() => _target?.SetActive(false);

        private void OnStartGameClicked() => StartGameClicked?.Invoke();

        private void OnExitClicked() => ExitClicked?.Invoke();
    }
}