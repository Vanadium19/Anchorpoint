using ComponentsModule;

namespace EffectModule
{
    public sealed class PrimaryBuffTargetService : IPrimaryBuffTargetService
    {
        public IEntity PrimaryTarget { get; private set; }

        public void SetPrimaryTarget(IEntity target)
        {
            if (ReferenceEquals(PrimaryTarget, target))
                return;

            PrimaryTarget = target;
        }
    }
}

