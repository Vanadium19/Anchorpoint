using Zenject;

namespace RandomEventsModule
{
    /// <summary>Serialized, polymorphic asset that creates an <see cref="IRandomEventChance"/> at runtime.</summary>
    public interface IRandomEventChanceAsset : IRandomEventAsset
    {
        /// <summary>Resolves the runtime chance through the given container.</summary>
        IRandomEventChance Create(DiContainer container);
    }
}
