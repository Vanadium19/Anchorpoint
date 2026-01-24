using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using EntityModule;
using PlayerModule.View;
using InputModule.Core;

namespace PlayerModule.Presenters
{
    public class GameOverPresenter : IInitializable, IDisposable
    {
        private readonly HealthComponent _health;
        private readonly GameOverView _view;
        private readonly IGameInput _input;

        public GameOverPresenter(HealthComponent health, GameOverView view, IGameInput input)
        {
            _health = health;
            _view = view;
            _input = input;
        }

        public void Initialize()
        {
            _view.Hide();
            _health.Died += OnDied;
            _view.RestartClicked += OnRestartClicked;
        }

        public void Dispose()
        {
            _health.Died -= OnDied;
            _view.RestartClicked -= OnRestartClicked;
        }

        private void OnDied()
        {
            _input.Disable();
            _view.Show();
        }

        private void OnRestartClicked()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }
}