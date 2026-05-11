namespace ComponentsModule
{
    public interface IEntity
    {
        T Get<T>() where T : class;
        bool TryGet<T>(out T value) where T : class;
    }
}
