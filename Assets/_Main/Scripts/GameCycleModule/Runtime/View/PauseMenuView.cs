using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCycleModule
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private SettingsMenuView settingsMenuView;

        public event Action SettingsClicked;
        public event Action MainMenuClicked;

        public SettingsMenuView SettingsMenuView => settingsMenuView;

        private GameObject Target => panel != null ? panel : gameObject;

        private void OnEnable()
        {
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDisable()
        {
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        public void Show() => Target.SetActive(true);

        public void Hide() => Target.SetActive(false);

        public void HideSettings() => settingsMenuView?.Hide();

        private void OnSettingsClicked() => SettingsClicked?.Invoke();

        private void OnMainMenuClicked() => MainMenuClicked?.Invoke();
    }
}
