using System;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Base class for a picker asset that resolves its runtime picker through Zenject.
    /// A concrete asset only needs to expose its own inspector fields and, if it depends on
    /// values that are not injectable, override <see cref="GetArguments"/>.
    /// </summary>
    [Serializable]
    public abstract class ZenjectRandomEventPickerAsset<TPicker> : IRandomEventPickerAsset
        where TPicker : IRandomEventPicker
    {
        /// <inheritdoc/>
        public virtual IRandomEventPicker Create(DiContainer container) => container.Instantiate<TPicker>(GetArguments(container));

        /// <summary>Extra constructor arguments Zenject cannot resolve on its own.</summary>
        protected virtual object[] GetArguments(DiContainer container) => Array.Empty<object>();
    }
}
