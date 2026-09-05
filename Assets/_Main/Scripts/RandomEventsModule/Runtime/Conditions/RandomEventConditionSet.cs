using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>A reusable "conditions + match mode" block for anything that needs to check conditions.</summary>
    [Serializable]
    public class RandomEventConditionSet
    {
        [SerializeField] private ConditionMatchType conditionMatch = ConditionMatchType.All;

        [SerializeReference] private IRandomEventConditionAsset[] conditions = Array.Empty<IRandomEventConditionAsset>();

        /// <summary>Resolves every non-null condition asset into one evaluatable group.</summary>
        public RandomEventConditionGroup Create(DiContainer container) =>
            new(conditions.CreateAll(asset => asset.Create(container)), conditionMatch);
    }
}
