using System;
using UnityEngine;

namespace FSMModule
{
    [Serializable]
    public class StateInfo<TKey, TContext>
    {
        [SerializeField] private TKey key;

        [SerializeReference] private IStateAsset<TContext> state;

        public TKey Key => key;
        public IStateAsset<TContext> State => state;
    }
}