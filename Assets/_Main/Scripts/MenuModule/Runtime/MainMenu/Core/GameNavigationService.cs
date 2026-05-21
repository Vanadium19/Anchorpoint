using SaveModule;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MenuModule
{
    public interface IGameNavigationService
    {
        void LoadHeadquarters();
        void LoadMainMenu(bool saveBeforeLoad = true);
        void QuitGame();
    }

    public class GameNavigationService : IGameNavigationService
    {
        private readonly GameNavigationConfig _config;
        private readonly IGameSaveLoader _gameSaveLoader;

        public GameNavigationService(
            GameNavigationConfig config,
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader)
        {
            _config = config;
            _gameSaveLoader = gameSaveLoader;
        }

        public void LoadHeadquarters()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(GetHeadquartersSceneName());
        }

        public void LoadMainMenu(bool saveBeforeLoad = true)
        {
            if (saveBeforeLoad)
                _gameSaveLoader?.Save();

            Time.timeScale = 1f;
            SceneManager.LoadScene(GetMainMenuSceneName());
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private string GetMainMenuSceneName() => _config != null
            ? _config.MainMenuSceneName
            : "MainMenu";

        private string GetHeadquartersSceneName() => _config != null
            ? _config.HeadquartersSceneName
            : "Camp";
    }
}
