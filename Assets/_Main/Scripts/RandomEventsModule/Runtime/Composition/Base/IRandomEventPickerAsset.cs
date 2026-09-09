using Zenject;

namespace RandomEventsModule
{
    /// <summary>Serialized, polymorphic asset that creates an <see cref="IRandomEventPicker"/> at runtime.</summary>
    public interface IRandomEventPickerAsset : IRandomEventAsset
    {
        /// <summary>Resolves the runtime picker through the given container.</summary>
        IRandomEventPicker Create(DiContainer container);
    }
}
