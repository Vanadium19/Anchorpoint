using System;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Base class for a chance asset that resolves its runtime chance through Zenject.
    /// A concrete asset only needs to expose its own inspector fields and, if it depends on
    /// values that are not injectable, override <see cref="GetArguments"/>.
    /// </summary>
    [Serializable]
    public abstract class ZenjectRandomEventChanceAsset<TChance> : IRandomEventChanceAsset
        where TChance : IRandomEventChance
    {
        /// <inheritdoc/>
        public virtual IRandomEventChance Create(DiContainer container) => container.Instantiate<TChance>(GetArguments(container));

        /// <summary>Extra constructor arguments Zenject cannot resolve on its own.</summary>
        protected virtual object[] GetArguments(DiContainer container) => Array.Empty<object>();
    }
}
