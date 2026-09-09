using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="ChanceGateAction"/>.</summary>
    [Serializable]
    public class ChanceGateActionAsset : ZenjectRandomEventActionAsset<ChanceGateAction>
    {
        [SerializeReference] private IRandomEventChanceAsset chance;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { chance?.Create(container) };
    }
}
