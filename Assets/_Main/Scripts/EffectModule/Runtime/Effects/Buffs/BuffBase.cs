using System;
using UnityEngine;

namespace EffectModule
{
    public abstract class BuffBase : IBuff
    {
        public event Action<IBuff> Completed;

        protected IBuffTarget Target { get; private set; }
        protected float ElapsedTime { get; private set; }

        public abstract string EffectId { get; }
        public abstract string BuffId { get; }

        public float Duration { get; protected set; }
        public float RemainingTime => Duration - ElapsedTime;
        public bool IsExpired => ElapsedTime >= Duration;
        public bool IsPaused { get; private set; }

        protected BuffBase(float duration)
        {
            Duration = duration;
        }

        public virtual void Apply(IBuffTarget target)
        {
            Target = target;
            ElapsedTime = 0f;
            OnApply();
        }

        public virtual void Tick(float deltaTime)
        {
            if (IsPaused || IsExpired)
                return;

            ElapsedTime += deltaTime;
            OnTick(deltaTime);

            if (IsExpired)
            {
                OnExpire();
                Completed?.Invoke(this);
            }
        }

        public virtual void Pause()
        {
            IsPaused = true;
            OnPause();
        }

        public virtual void Resume()
        {
            IsPaused = false;
            OnResume();
        }

        public virtual void Cancel()
        {
            OnCancel();
            Target = null;
        }

        public virtual void ResetDuration()
        {
            ElapsedTime = 0f;
        }

        public void SetElapsedTime(float elapsed)
        {
            ElapsedTime = Mathf.Clamp(elapsed, 0f, Duration);
        }

        protected virtual void OnApply() { }
        protected virtual void OnTick(float deltaTime) { }
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnExpire() { }
        protected virtual void OnCancel() { }
    }
}
