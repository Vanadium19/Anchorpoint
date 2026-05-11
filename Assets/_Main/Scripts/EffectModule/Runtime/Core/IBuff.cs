using System;
using ComponentsModule;

namespace EffectModule
{
    public interface IBuff : IEffect
    {
        event Action<IBuff> Completed;

        string BuffId { get; }
        float Duration { get; }
        float RemainingTime { get; }
        bool IsExpired { get; }
        bool IsPaused { get; }

        void Apply(IEntity target);
        void Tick(float deltaTime);
        void Pause();
        void Resume();
        void Cancel();
        void ResetDuration();
        void SetElapsedTime(float elapsed);
    }
}
