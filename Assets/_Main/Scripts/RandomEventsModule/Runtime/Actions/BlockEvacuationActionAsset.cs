using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="BlockEvacuationAction"/>.</summary>
    [Serializable]
    public class BlockEvacuationActionAsset : ZenjectRandomEventActionAsset<BlockEvacuationAction>
    {
        [SerializeField] private string blockedMessageKey;
        [SerializeField] [Min(0f)] private float messageDurationSeconds = 3f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) =>
            new object[] { blockedMessageKey, messageDurationSeconds };
    }
}
