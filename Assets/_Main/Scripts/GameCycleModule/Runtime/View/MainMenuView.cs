using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCycleModule
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [SerializeField] private SettingsMenuView settingsMenuView;

        public event Action StartGameClicked;
        public event Action SettingsClicked;
        public event Action ExitClicked;

        public SettingsMenuView SettingsMenuView => settingsMenuView;

        private GameObject _target => panel ? panel : gameObject;

        private void OnEnable()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnDisable()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);
        }

        public void Show() => _target.SetActive(true);

        public void Hide() => _target.SetActive(false);

        private void OnStartGameClicked() => StartGameClicked?.Invoke();

        private void OnSettingsClicked() => SettingsClicked?.Invoke();

        private void OnExitClicked() => ExitClicked?.Invoke();
    }
}