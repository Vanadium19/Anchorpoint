using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    internal sealed class AudioEventBehaviour_PitchCurve : AudioEventProgressBehaviour
    {
        [SerializeField]
        private AnimationCurve curve;

        private float _startPitch;

        protected override void OnProgressStart(in AudioSourceEvent evt)
        {
            _startPitch = evt.Source.pitch;
        }

        protected override void OnProgressUpdate(in AudioSourceEvent evt, in float behaviourProgress)
        {
            evt.Source.pitch = this.curve.Evaluate(behaviourProgress);
        }

        protected override void OnProgressStop(in AudioSourceEvent evt)
        {
            evt.Source.pitch = _startPitch;
        }
    }
}