using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FSMModule
{
    [Serializable]
    public abstract class AutoStateMachineAsset<TKey, TContext> : IAutoStateMachineAsset<TContext>
    {
        [SerializeField] private TKey initialState;
        [SerializeField] private StateInfo<TKey, TContext>[] states;

        [SerializeReference] private IStateTransitionAsset<TKey, TContext>[] transitions;

        public IState Create(TContext context)
        {
            var stateMachine = new StateMachine<TKey>(initialState, states.Select(state => new KeyValuePair<TKey, IState>(state.Key, state.State.Create(context))));
            var machineTransitions = transitions.Select(asset => asset.Create(context));
            return new AutoStateMachine<TKey>(stateMachine, machineTransitions);
        }
    }
}