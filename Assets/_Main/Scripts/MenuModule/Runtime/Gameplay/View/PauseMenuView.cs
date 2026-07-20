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

        public void Show() => SetTargetActive(true);

        public void Hide() => SetTargetActive(false);

        private void SetTargetActive(bool isActive)
        {
            if (this == null)
                return;

            if (panel != null)
            {
                panel.SetActive(isActive);
                return;
            }

            gameObject.SetActive(isActive);
        }

        private void OnMainMenuClicked() => MainMenuClicked?.Invoke();
    }
}
