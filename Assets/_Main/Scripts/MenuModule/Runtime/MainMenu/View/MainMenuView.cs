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

        [Header("Language")]
        [SerializeField] private Button englishButton;
        [SerializeField] private Button russianButton;

        public event Action StartGameClicked;
        public event Action ExitClicked;
        public event Action EnglishClicked;
        public event Action RussianClicked;

        private GameObject _target => panel ? panel : gameObject;

        private void OnEnable()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            if (englishButton != null)
                englishButton.onClick.AddListener(OnEnglishClicked);

            if (russianButton != null)
                russianButton.onClick.AddListener(OnRussianClicked);
        }

        private void OnDisable()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);

            if (englishButton != null)
                englishButton.onClick.RemoveListener(OnEnglishClicked);

            if (russianButton != null)
                russianButton.onClick.RemoveListener(OnRussianClicked);
        }

        public void Show() => _target?.SetActive(true);

        public void Hide() => _target?.SetActive(false);

        private void OnStartGameClicked() => StartGameClicked?.Invoke();

        private void OnExitClicked() => ExitClicked?.Invoke();

        private void OnEnglishClicked() => EnglishClicked?.Invoke();

        private void OnRussianClicked() => RussianClicked?.Invoke();
    }
}