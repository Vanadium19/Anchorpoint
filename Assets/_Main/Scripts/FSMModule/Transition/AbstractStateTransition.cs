namespace FSMModule
{
    public abstract class AbstractStateTransition<TKey> : IStateTransition<TKey>
    {
        protected AbstractStateTransition(TKey from, TKey to)
        {
            From = from;
            To = to;
        }

        public TKey From { get; }
        public TKey To { get; }

        public abstract bool CanPerform();
    }
}