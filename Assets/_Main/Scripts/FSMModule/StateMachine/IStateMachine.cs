using System;
using System.Collections.Generic;

namespace FSMModule
{
    public interface IStateMachine<TKey> : IState
    {
        event Action<TKey> OnStateAdded;
        event Action<TKey> OnStateChanged;
        event Action<TKey> OnStateRemoved;

        int StateCount { get; }
        TKey CurrentState { get; }
        IReadOnlyCollection<TKey> States { get; }

        bool AddState(TKey key, IState state);
        bool RemoveState(TKey key);
        bool ContainsState(TKey key);

        bool TryChangeState(TKey key);
        void ChangeState(TKey key);
    }
}