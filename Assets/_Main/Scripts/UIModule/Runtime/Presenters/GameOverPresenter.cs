using ComponentsModule;
using Core.Services;
using System;
using UIModule;
using Zenject;

namespace UIModule
{
    public class GameOverPresenter : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly GameOverView _view;
        private readonly IGameSessionService _sessionService;

        public GameOverPresenter(
            IHealthComponent health,
            GameOverView view,
            IGameSessionService sessionService)
        {
            _health = health;
            _view = view;
            _sessionService = sessionService;
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
            _view.Show();
        }

        private void OnRestartClicked()
        {
            _sessionService.RestartLevel();
        }
    }
}