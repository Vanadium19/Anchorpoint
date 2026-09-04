using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// A reusable "conditions + match mode" block for any gate action that needs to check
    /// conditions instead of redeclaring the same fields on every one of them.
    /// </summary>
    [Serializable]
    public class RandomEventConditionSet
    {
        [SerializeField] private ConditionMatchType conditionMatch = ConditionMatchType.All;

        [SerializeReference] private IRandomEventConditionAsset[] conditions = Array.Empty<IRandomEventConditionAsset>();

        /// <summary>How the created conditions combine into one pass/fail result.</summary>
        public ConditionMatchType Match => conditionMatch;

        /// <summary>Resolves every non-null condition asset through the given container.</summary>
        public IReadOnlyList<IRandomEventCondition> Create(DiContainer container) =>
            conditions.CreateAll(asset => asset.Create(container));
    }
}
