namespace FSMModule
{
    public interface IStateTransition<out TKey>
    {
        TKey From { get; }
        TKey To { get; }

        bool CanPerform();
    }
}