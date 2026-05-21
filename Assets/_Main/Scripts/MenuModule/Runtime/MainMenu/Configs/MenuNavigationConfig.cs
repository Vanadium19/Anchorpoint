using UnityEngine;

namespace MenuModule
{
    [CreateAssetMenu(fileName = "MenuNavigationConfig", menuName = "Game/Configs/Menu/MenuNavigationConfig")]
    public class MenuNavigationConfig : ScriptableObject
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string startGameSceneName = "Camp";

        public string MainMenuSceneName => string.IsNullOrWhiteSpace(mainMenuSceneName) ? "MainMenu" : mainMenuSceneName;

        public string CampSceneName => string.IsNullOrWhiteSpace(startGameSceneName) ? "Camp" : startGameSceneName;
    }
}