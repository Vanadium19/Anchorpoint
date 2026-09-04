using System;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Base class for an action asset that resolves its runtime action through Zenject.
    /// A concrete asset only needs to expose its own inspector fields and, if it depends on
    /// values that are not injectable, override <see cref="GetArguments"/>.
    /// </summary>
    [Serializable]
    public abstract class ZenjectRandomEventActionAsset<TAction> : IRandomEventActionAsset
        where TAction : IRandomEventAction
    {
        /// <inheritdoc/>
        public IRandomEventAction Create(DiContainer container) => container.Instantiate<TAction>(GetArguments());

        /// <summary>Extra constructor arguments Zenject cannot resolve on its own.</summary>
        protected virtual object[] GetArguments() => Array.Empty<object>();
    }
}
