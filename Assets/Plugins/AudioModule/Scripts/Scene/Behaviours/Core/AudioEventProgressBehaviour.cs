using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    public abstract class AudioEventProgressBehaviour : AudioEventBehaviour
    {
        [SerializeField]
        private bool fullTime = true;

        [SerializeReference, HideIf(nameof(fullTime))]
        private IBehaviourProgressStrategy progressStrategy = new TimeBehaviourProgressStrategy();

        private bool _isProgressActive;

        public override sealed void OnStart(in AudioSourceEvent evt)
        {
            _isProgressActive = false;
        }

        public override sealed void OnUpdate(in AudioSourceEvent evt)
        {
            if (!this.IsActive(evt))
            {
                this.StopProgress(evt);
                return;
            }

            if (!_isProgressActive)
            {
                _isProgressActive = true;
                this.OnProgressStart(evt);
            }

            float progress = this.GetProgress(evt);
            this.OnProgressUpdate(evt, progress);
        }

        public override sealed void OnStop(in AudioSourceEvent evt)
        {
            this.StopProgress(evt);
        }

        public override sealed void OnReset(in AudioSourceEvent evt)
        {
            this.StopProgress(evt);
        }

        protected virtual void OnProgressStart(in AudioSourceEvent evt) { }

        protected virtual void OnProgressUpdate(in AudioSourceEvent evt, in float behaviourProgress) { }

        protected virtual void OnProgressStop(in AudioSourceEvent evt) { }

        private void StopProgress(in AudioSourceEvent evt)
        {
            if (!_isProgressActive)
                return;

            _isProgressActive = false;
            this.OnProgressStop(evt);
        }

        private float GetProgress(in AudioSourceEvent evt)
        {
            return this.fullTime ? evt.CurrentProgress : GetStrategy().GetProgress(evt);
        }

        private bool IsActive(in AudioSourceEvent evt)
        {
            return this.fullTime || GetStrategy().IsActive(evt);
        }

        private IBehaviourProgressStrategy GetStrategy()
        {
            this.progressStrategy ??= new TimeBehaviourProgressStrategy();
            return this.progressStrategy;
        }
    }

    internal interface IBehaviourProgressStrategy
    {
        bool IsActive(in AudioSourceEvent evt);

        float GetProgress(in AudioSourceEvent evt);
    }

    [Serializable]
    internal sealed class TimeBehaviourProgressStrategy : IBehaviourProgressStrategy
    {
        [SerializeField]
        private float startTime;

        [SerializeField]
        private float endTime;

        public bool IsActive(in AudioSourceEvent evt)
        {
            return evt.CurrentTime >= this.startTime && evt.CurrentTime <= this.endTime;
        }

        public float GetProgress(in AudioSourceEvent evt)
        {
            return Mathf.InverseLerp(this.startTime, this.endTime, evt.CurrentTime);
        }
    }

    [Serializable]
    internal sealed class ProgressBehaviourProgressStrategy : IBehaviourProgressStrategy
    {
        [SerializeField, Range(0, 1)]
        private float startProgress;

        [SerializeField, Range(0, 1)]
        private float endProgress = 1;

        public bool IsActive(in AudioSourceEvent evt)
        {
            return evt.CurrentProgress >= this.startProgress && evt.CurrentProgress <= this.endProgress;
        }

        public float GetProgress(in AudioSourceEvent evt)
        {
            return Mathf.InverseLerp(this.startProgress, this.endProgress, evt.CurrentProgress);
        }
    }
}