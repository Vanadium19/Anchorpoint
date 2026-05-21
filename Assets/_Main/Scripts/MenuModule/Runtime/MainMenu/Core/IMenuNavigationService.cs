namespace MenuModule
{
    public interface IMenuNavigationService
    {
        void LoadGameFromMenu();
        void LoadMainMenu(bool saveBeforeLoad = true);
        void QuitGame();
    }
}