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

        public IEffectHandle Fire(EffectId effectId, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null)
        {
            var effects = _effects[effectId];

            EffectView effect = null;

            // Skip pooled effects whose GameObject was destroyed (e.g. with the building they were parented to).
            while (effect == null && effects.TryDequeue(out var pooled))
                effect = pooled;

            if (effect == null)
                effect = Spawn(effectId);

            effect.Finished -= OnEffectFinished;
            effect.Finished += OnEffectFinished;
            effect.transform.SetParent(parent != null ? parent : _container, true);
            effect.transform.SetPositionAndRotation(position, rotation);
            effect.SetScale(scale ?? Vector3.one);
            effect.gameObject.SetActive(true);
            effect.Play();

            return effect;
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
            if (effect == null)
                return;

            effect.Finished -= OnEffectFinished;
            effect.gameObject.SetActive(false);
            effect.transform.SetParent(_container, true);
            _effects[effect.Id].Enqueue(effect);
        }
    }
}
