using System;
using System.Collections.Generic;

namespace FSMModule
{
    public interface IAutoStateMachine<TKey> : IStateMachine<TKey>
    {
        event Action<IStateTransition<TKey>> OnTransitionAdded;
        event Action<IStateTransition<TKey>> OnTransitionRemoved;

        int TransitionCount { get; }
        IEnumerable<(TKey, TKey)> Transitions { get; }

        bool AddTransition(IStateTransition<TKey> transition);
        bool AddTransition(TKey from, TKey to, Func<bool> condition);

        bool RemoveTransition(IStateTransition<TKey> transition);
        bool RemoveTransition(TKey from, TKey to);

        bool ContainsTransition(IStateTransition<TKey> transition);
        bool ContainsTransition(TKey from, TKey to);
    }
}