using System;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Base class for an action asset that resolves its runtime action through Zenject.
    /// A concrete asset only needs to expose its own inspector fields and, if it depends on
    /// values that are not injectable, override <see cref="GetArguments"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="GetArguments"/> receives the container, so an asset can build nested runtime
    /// objects — a <see cref="RandomEventActionPlan"/> for a branch, a
    /// <see cref="RandomEventConditionGroup"/> for a check — and pass them as constructor arguments.
    /// </remarks>
    [Serializable]
    public abstract class ZenjectRandomEventActionAsset<TAction> : IRandomEventActionAsset
        where TAction : IRandomEventAction
    {
        /// <inheritdoc/>
        public virtual IRandomEventAction Create(DiContainer container) => container.Instantiate<TAction>(GetArguments(container));

        /// <summary>Extra constructor arguments Zenject cannot resolve on its own.</summary>
        protected virtual object[] GetArguments(DiContainer container) => Array.Empty<object>();
    }
}
