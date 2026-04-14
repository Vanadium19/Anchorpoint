namespace SaveModule
{
    public interface IGameRepository
    {
        TData Load<TData>(string filePath);
        void Save<TData>(TData data, string filePath);
    }
}
