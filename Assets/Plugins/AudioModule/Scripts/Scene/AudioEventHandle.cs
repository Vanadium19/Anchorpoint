using System;
using UnityEngine;

namespace AudioModule
{
    public readonly struct AudioEventHandle
    {
        private readonly AudioEventBase audioEvent;
        private readonly AudioSystem audioSystem;

        public string Id
        {
            get { return this.audioEvent.Id; }
        }

        public bool IsPlaying
        {
            get { return this.audioSystem.IsPlayingEvent(this.audioEvent); }
        }

        public bool IsValid
        {
            get { return this.audioEvent != null && 
                         this.audioEvent.IsValid &&
                         this.audioSystem.IsCreatedEvent(this.audioEvent); }
        }

        internal AudioEventHandle(AudioEventBase audioEvent, AudioSystem audioSystem)
        {
            this.audioEvent = audioEvent;
            this.audioSystem = audioSystem;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (this.IsValid)
            {
                this.audioEvent.Position = position;
                this.audioEvent.Rotation = rotation;
            }
        }

        public void SetPosition(Vector3 position)
        {
            if (this.IsValid) 
                this.audioEvent.Position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            if (this.IsValid) 
                this.audioEvent.Rotation = rotation;
        }
        
        public void Play(Vector3 position, Quaternion rotation)
        {
            if (this.IsValid)
            {
                this.audioEvent.Position = position;
                this.audioEvent.Rotation = rotation;
                this.audioSystem.StartEvent(this.audioEvent);
            }
        }

        public void Play()
        {
            if (this.IsValid)
                this.audioSystem.StartEvent(this.audioEvent);
        }

        public void Stop()
        {
            if (this.IsValid)
                this.audioSystem.StopEvent(this.audioEvent);
        }

        public void Stop(float fadeoutTime)
        {
            if (this.IsValid) 
                this.audioSystem.StopEvent(this.audioEvent, fadeoutTime);
        }

        public void Stop(float fadeoutTime, AnimationCurve curve)
        {
            if (this.IsValid) 
                this.audioSystem.StopEvent(this.audioEvent, fadeoutTime, curve);
        }

        public void Dispose()
        {
            if (this.IsValid)
                this.audioSystem.DisposeEvent(this.audioEvent);
        }
        
        public void Dispose(float fadeoutTime)
        {
            if (this.IsValid)
                this.audioSystem.DisposeEvent(this.audioEvent.Id, fadeoutTime);
        }
        
        public void Dispose(float fadeoutTime, AnimationCurve curve)
        {
            if (this.IsValid)
                this.audioSystem.DisposeEvent(this.audioEvent.Id, curve, fadeoutTime);
        }
    }
}