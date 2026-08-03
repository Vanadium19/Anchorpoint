using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VFXModule
{
    public class EffectsService : IEffectsService
    {
        private readonly Transform _container;
        private readonly EffectsCatalog _catalog;

        private readonly Dictionary<EffectId, Queue<EffectView>> _effects = new();

        public EffectsService(Transform container, EffectsCatalog catalog)
        {
            _container = container;
            _catalog = catalog;

            foreach (EffectId id in Enum.GetValues(typeof(EffectId)))
                _effects[id] = new();
        }

        public void Fire(EffectId effectId, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var effects = _effects[effectId];

            if (!effects.TryDequeue(out var effect))
                effect = Spawn(effectId);

            effect.Finished -= OnEffectFinished;
            effect.Finished += OnEffectFinished;
            effect.transform.SetParent(parent != null ? parent : _container, true);
            effect.transform.SetPositionAndRotation(position, rotation);
            effect.gameObject.SetActive(true);
            effect.Play();
        }

        private EffectView Spawn(EffectId effectId)
        {
            var prefab = _catalog.GetPrefab(effectId);
            var effect = Object.Instantiate(prefab, _container);
            effect.Initialize(effectId);
            return effect;
        }

        private void OnEffectFinished(EffectView effect)
        {
            effect.Finished -= OnEffectFinished;
            effect.gameObject.SetActive(false);
            effect.transform.SetParent(_container, true);
            _effects[effect.Id].Enqueue(effect);
        }
    }
}
