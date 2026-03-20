namespace FSMModule
{
    public interface IStateTransitionAsset<out TKey, in TContext>
    {
        IStateTransition<TKey> Create(TContext context);
    }
}