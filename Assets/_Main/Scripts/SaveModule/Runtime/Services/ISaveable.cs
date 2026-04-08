namespace SaveModule
{
    public interface ISaveable
    {
        string SaveKey { get; }
        string CreateMemento();
        void RestoreMemento(string data);
    }
}
