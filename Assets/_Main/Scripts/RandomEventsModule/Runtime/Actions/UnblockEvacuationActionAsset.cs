using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="UnblockEvacuationAction"/>.</summary>
    [Serializable]
    public class UnblockEvacuationActionAsset : ZenjectRandomEventActionAsset<UnblockEvacuationAction>
    {
        [SerializeField] private string blockedMessageKey;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { blockedMessageKey };
    }
}
