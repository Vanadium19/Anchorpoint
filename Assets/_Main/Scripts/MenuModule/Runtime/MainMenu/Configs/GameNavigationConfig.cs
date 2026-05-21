using UnityEngine;

namespace MenuModule
{
    [CreateAssetMenu(fileName = "GameNavigationConfig", menuName = "Game/Configs/GameNavigationConfig")]
    public class GameNavigationConfig : ScriptableObject
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string headquartersSceneName = "Camp";

        public string MainMenuSceneName => string.IsNullOrWhiteSpace(mainMenuSceneName)
            ? "MainMenu"
            : mainMenuSceneName;

        public string HeadquartersSceneName => string.IsNullOrWhiteSpace(headquartersSceneName)
            ? "Camp"
            : headquartersSceneName;
    }
}
