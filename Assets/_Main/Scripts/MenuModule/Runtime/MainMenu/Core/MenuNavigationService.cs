using SaveModule;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MenuModule
{
    public class MenuNavigationService : IMenuNavigationService
    {
        private readonly MenuNavigationConfig _config;
        private readonly IGameSaveLoader _gameSaveLoader;

        public MenuNavigationService(MenuNavigationConfig config,
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader)
        {
            _config = config;
            _gameSaveLoader = gameSaveLoader;
        }

        public void LoadGameFromMenu()
        {
            Time.timeScale = 1f;
            var sceneName = _config.CampSceneName;
            SceneManager.LoadScene(sceneName);
        }

        public void LoadMainMenu(bool saveBeforeLoad = true)
        {
            if (saveBeforeLoad)
                _gameSaveLoader?.Save();

            Time.timeScale = 1f;
            var sceneName = _config.MainMenuSceneName;
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}