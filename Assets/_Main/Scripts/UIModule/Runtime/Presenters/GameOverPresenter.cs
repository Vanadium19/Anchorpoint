using System;
using UnityEngine.SceneManagement;
using Zenject;
using ComponentsModule;
using InputModule.Core;

namespace UIModule
{
    public class GameOverPresenter : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly GameOverView _view;
        private readonly IInputMap _input;

        public GameOverPresenter(IHealthComponent health, GameOverView view, IInputMap input)
        {
            _health = health;
            _input = input;
            _view = view;
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

        //FIXME: Бизнес логика в презентере
        private void OnDied()
        {
            _input.Disable();
            _view.Show();
        }

        //FIXME: Бизнес логика в презентере
        private void OnRestartClicked()
        {
            var currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }
}