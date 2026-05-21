using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioModule
{
    [Serializable]
    public sealed class AudioLowPassFilter : AudioEventProgressBehaviour
    {
        [SerializeReference] private IFloatProvider cutoffFrequency = new FloatConst(5007.7f);
        [SerializeReference] private IFloatProvider lowpassResonanceQ = new FloatConst(1);

        private UnityEngine.AudioLowPassFilter _filter;

        protected override void OnProgressStart(in AudioSourceEvent evt)
        {
            _filter = evt.Source.gameObject.AddComponent<UnityEngine.AudioLowPassFilter>();
            _filter.cutoffFrequency = this.cutoffFrequency?.Value ?? 5007.7f;
            _filter.lowpassResonanceQ = this.lowpassResonanceQ?.Value ?? 1;
        }

        protected override void OnProgressStop(in AudioSourceEvent evt)
        {
            if (_filter != null)
                DestroyFilter();
        }

        private void DestroyFilter()
        {
            Object.Destroy(_filter);
            _filter = null;
        }
    }
}