using Zenject;

namespace RandomEventsModule
{
    /// <summary>Serialized, polymorphic asset that creates an <see cref="IRandomEventAction"/> at runtime.</summary>
    public interface IRandomEventActionAsset : IRandomEventAsset
    {
        /// <summary>Resolves the runtime action through the given container.</summary>
        IRandomEventAction Create(DiContainer container);
    }
}
