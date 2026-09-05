using Zenject;

namespace RandomEventsModule
{
    /// <summary>Serialized, polymorphic asset that creates an <see cref="IRandomEventTriggerSource"/> at runtime.</summary>
    public interface IRandomEventTriggerSourceAsset : IRandomEventAsset
    {
        /// <summary>Resolves the runtime trigger source through the given container.</summary>
        IRandomEventTriggerSource Create(DiContainer container);
    }
}
