using Zenject;

namespace RandomEventsModule
{
    /// <summary>Serialized, polymorphic asset that creates an <see cref="IRandomEventCondition"/> at runtime.</summary>
    public interface IRandomEventConditionAsset : IRandomEventAsset
    {
        /// <summary>Resolves the runtime condition through the given container.</summary>
        IRandomEventCondition Create(DiContainer container);
    }
}
