using System;
using UnityEngine;
using UnityEngine.UI;

namespace MenuModule
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button mainMenuButton;

        public event Action MainMenuClicked;

        private GameObject _target => panel != null ? panel : gameObject;

        private void OnEnable()
        {
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDisable()
        {
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        public void Show() => _target?.SetActive(true);

        public void Hide() => _target?.SetActive(false);

        private void OnMainMenuClicked() => MainMenuClicked?.Invoke();
    }
}