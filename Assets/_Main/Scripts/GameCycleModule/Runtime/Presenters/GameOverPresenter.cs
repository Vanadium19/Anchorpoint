using ComponentsModule;
using System;
using InputModule;
using PlayerModule;
using Zenject;

namespace GameCycleModule
{
    public class GameOverPresenter : IInitializable, IDisposable
    {
        private readonly PlayerProvider _player;
        private readonly GameOverView _view;

        private readonly IGameSessionService _sessionService;
        private readonly IInputService _inputService;

        public GameOverPresenter(PlayerProvider player,
            GameOverView view,
            IGameSessionService sessionService,
            IInputService inputService)
        {
            _player = player;
            _view = view;

            _sessionService = sessionService;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _view.Hide();
            _player.Get<IHealthComponent>().Died += OnDied;
            _view.RestartClicked += OnRestartClicked;
        }

        public void Dispose()
        {
            _player.Get<IHealthComponent>().Died -= OnDied;
            _view.RestartClicked -= OnRestartClicked;
        }

        private void OnDied()
        {
            _inputService.Disable();
            _view.Show();
        }

        private void OnRestartClicked()
        {
            _sessionService.RestartLevel();
        }
    }
}