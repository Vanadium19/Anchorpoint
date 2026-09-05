using System;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Base class for a condition asset that resolves its runtime condition through Zenject.
    /// A concrete asset only needs to expose its own inspector fields and, if it depends on
    /// values that are not injectable, override <see cref="GetArguments"/>.
    /// </summary>
    [Serializable]
    public abstract class ZenjectRandomEventConditionAsset<TCondition> : IRandomEventConditionAsset
        where TCondition : IRandomEventCondition
    {
        /// <inheritdoc/>
        public virtual IRandomEventCondition Create(DiContainer container) => container.Instantiate<TCondition>(GetArguments(container));

        /// <summary>Extra constructor arguments Zenject cannot resolve on its own.</summary>
        protected virtual object[] GetArguments(DiContainer container) => Array.Empty<object>();
    }
}
