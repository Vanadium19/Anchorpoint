namespace EffectModule
{
    public interface IEffectTarget
    {
        bool TryGet<T>(out T component) where T : class;
    }
}
