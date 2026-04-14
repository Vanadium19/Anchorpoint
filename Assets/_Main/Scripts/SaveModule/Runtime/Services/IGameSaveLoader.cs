namespace SaveModule
{
    public interface IGameSaveLoader
    {
        void Save();
        void Load();
        void RegisterSaveable(ISaveable saveable);
        void UnregisterSaveable(ISaveable saveable);
    }
}
