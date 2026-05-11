using ComponentsModule;

namespace EffectModule
{
    public interface IPrimaryBuffTargetService
    {
        IEntity PrimaryTarget { get; }
        void SetPrimaryTarget(IEntity target);
    }
}
