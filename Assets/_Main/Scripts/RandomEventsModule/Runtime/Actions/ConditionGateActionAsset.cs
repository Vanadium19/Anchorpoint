using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="ConditionGateAction"/>.</summary>
    [Serializable]
    public class ConditionGateActionAsset : ZenjectRandomEventActionAsset<ConditionGateAction>
    {
        [SerializeField] private RandomEventConditionSet conditions = new();

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { conditions.Create(container) };
    }
}
