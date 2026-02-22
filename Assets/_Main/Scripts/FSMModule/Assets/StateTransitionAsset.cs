using System;
using System.Linq;
using FSMModule;
using UnityEngine;

namespace AIModule
{
    [Serializable]
    public abstract class StateTransitionAsset<TKey, TContext> : IStateTransitionAsset<TKey, TContext>
    {
        [SerializeField] private TKey from;
        [SerializeField] private TKey to;

        [SerializeField] private ConditionType conditionType = ConditionType.All;

        [SerializeReference] private IConditionAsset<TContext>[] conditions;

        public IStateTransition<TKey> Create(TContext context) => new StateTransition<TKey>(from, to, CreateCondition(context));

        private Func<bool> CreateCondition(TContext context) => conditions.Length switch
        {
            0 => () => true,
            1 => conditions[0].Create(context),
            var _ => conditionType == ConditionType.All ? CreateAllCondition(context) : CreateAnyCondition(context),
        };

        private Func<bool> CreateAllCondition(TContext context)
        {
            var allConditions = conditions.Select(asset => asset.Create(context)).ToArray();
            return () => allConditions.All(condition => condition.Invoke());
        }

        private Func<bool> CreateAnyCondition(TContext context)
        {
            var anyConditions = conditions.Select(asset => asset.Create(context)).ToArray();
            return () => anyConditions.Any(condition => condition.Invoke());
        }
    }
}