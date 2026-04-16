namespace EffectModule
{
    public sealed class PrimaryBuffTargetService : IPrimaryBuffTargetService
    {
        public IBuffTarget PrimaryTarget { get; private set; }

        public void SetPrimaryTarget(IBuffTarget target)
        {
            if (ReferenceEquals(PrimaryTarget, target))
                return;

            PrimaryTarget = target;
        }
    }
}

