using System;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    internal sealed class AudioEventBehaviour_VolumeCurve : AudioEventProgressBehaviour
    {
        [SerializeField]
        private AnimationCurve curve;

        private float _startVolume;

        protected override void OnProgressStart(in AudioSourceEvent evt)
        {
            _startVolume = evt.Source.volume;
        }

        protected override void OnProgressUpdate(in AudioSourceEvent evt, in float behaviourProgress)
        {
            evt.Source.volume = this.curve.Evaluate(behaviourProgress);
        }

        protected override void OnProgressStop(in AudioSourceEvent evt)
        {
            evt.Source.volume = _startVolume;
        }
    }
}