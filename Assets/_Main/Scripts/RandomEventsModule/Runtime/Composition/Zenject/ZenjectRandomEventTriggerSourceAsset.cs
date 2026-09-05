using System;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Base class for a trigger source asset that resolves its runtime source through Zenject.
    /// A concrete asset only needs to expose its own inspector fields and, if it depends on
    /// values that are not injectable, override <see cref="GetArguments"/>.
    /// </summary>
    [Serializable]
    public abstract class ZenjectRandomEventTriggerSourceAsset<TSource> : IRandomEventTriggerSourceAsset
        where TSource : IRandomEventTriggerSource
    {
        /// <inheritdoc/>
        public virtual IRandomEventTriggerSource Create(DiContainer container) => container.Instantiate<TSource>(GetArguments(container));

        /// <summary>Extra constructor arguments Zenject cannot resolve on its own.</summary>
        protected virtual object[] GetArguments(DiContainer container) => Array.Empty<object>();
    }
}
