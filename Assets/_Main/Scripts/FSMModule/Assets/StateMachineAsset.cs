using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FSMModule
{
    [Serializable]
    public abstract class StateMachineAsset<TKey, TContext> : IStateMachineAsset<TContext>
    {
        [SerializeField] private TKey initialState;
        [SerializeField] private StateInfo<TKey, TContext>[] states;

        public IState Create(TContext context) => new StateMachine<TKey>(initialState,
            states.Select(state => new KeyValuePair<TKey, IState>(state.Key, state.State.Create(context))));
    }
}