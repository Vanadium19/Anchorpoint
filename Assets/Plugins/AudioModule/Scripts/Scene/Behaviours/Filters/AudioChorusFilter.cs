using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioModule
{
    [Serializable]
    public sealed class AudioChorusFilter : AudioEventProgressBehaviour
    {
        [SerializeReference] private IFloatProvider dryMix = new FloatConst(0.5f);
        [SerializeReference] private IFloatProvider delay = new FloatConst(40);
        [SerializeReference] private IFloatProvider rate = new FloatConst(0.8f);
        [SerializeReference] private IFloatProvider depth = new FloatConst(0.03f);

        [Space]
        [SerializeReference] private IFloatProvider wetMix1 = new FloatConst(0.5f);
        [SerializeReference] private IFloatProvider wetMix2 = new FloatConst(0.5f);
        [SerializeReference] private IFloatProvider wetMix3 = new FloatConst(0.5f);

        private UnityEngine.AudioChorusFilter _filter;

        protected override void OnProgressStart(in AudioSourceEvent evt)
        {
            _filter = evt.Source.gameObject.AddComponent<UnityEngine.AudioChorusFilter>();

            _filter.dryMix = this.dryMix?.Value ?? 0.5f;
            _filter.delay = this.delay?.Value ?? 40;
            _filter.rate = this.rate?.Value ?? 0.8f;
            _filter.depth = this.depth?.Value ?? 0.03f;

            _filter.wetMix1 = wetMix1?.Value ?? 0.5f;
            _filter.wetMix2 = wetMix2?.Value ?? 0.5f;
            _filter.wetMix3 = wetMix3?.Value ?? 0.5f;
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