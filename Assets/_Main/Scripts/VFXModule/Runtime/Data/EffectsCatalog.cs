using System;
using System.Linq;
using UnityEngine;

namespace VFXModule
{
    [CreateAssetMenu(fileName = "EffectsCatalog", menuName = "Game/Configs/EffectsCatalog")]
    public class EffectsCatalog : ScriptableObject
    {
        [SerializeField] private EffectData[] _effects;

        public EffectView GetPrefab(EffectId id)
        {
            var data = _effects.FirstOrDefault(data => data.Id == id);

            if (data == null)
                throw new Exception($"Effect with id {id} not found in catalog");

            return data.Prefab;
        }
    }
}