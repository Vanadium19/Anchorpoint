using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// One entry in an event's flat action sequence; a run of consecutive steps with
    /// <see cref="RunInParallel"/> set forms one parallel group.
    /// </summary>
    /// <remarks>Edited through <see cref="RandomEventDefinitionEditor"/>, not the generic list drawer.</remarks>
    [Serializable]
    public class RandomEventActionStep
    {
        [SerializeField] private bool runInParallel;
        [SerializeReference] private IRandomEventActionAsset action;

        /// <summary>Whether this step runs alongside the previous one instead of after it.</summary>
        public bool RunInParallel => runInParallel;

        /// <summary>Resolves this step's action through the given container.</summary>
        public IRandomEventAction Create(DiContainer container) => action?.Create(container);
    }
}
