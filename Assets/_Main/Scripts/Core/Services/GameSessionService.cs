using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Zenject;
using ComponentsModule;
using InputModule.Core;

namespace Core.Services
{
    public class GameSessionService : IGameSessionService, IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly IInputMap _input;

        public GameSessionService(IHealthComponent health, IInputMap input)
        {
            _health = health;
            _input = input;
        }

        public void Initialize()
        {
            _health.Died += OnPlayerDied;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Dispose()
        {
            _health.Died -= OnPlayerDied;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnPlayerDied()
        {
            _input.Disable();
        }
        public void RestartLevel()
        {
            var currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }
}