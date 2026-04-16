namespace EffectModule
{
    public interface IPrimaryBuffTargetService
    {
        IBuffTarget PrimaryTarget { get; }
        void SetPrimaryTarget(IBuffTarget target);
    }
}

