using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Zenject;

namespace GameCycleModule
{
    public class GameSessionService : IGameSessionService, IInitializable, IDisposable
    {
        public void Initialize()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Dispose()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartLevel()
        {
            var currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }
}