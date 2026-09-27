using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace VFXModule
{
    [CreateAssetMenu(fileName = "ScreenEffectsCatalog", menuName = "Game/Configs/ScreenEffectsCatalog")]
    public class ScreenEffectsCatalog : ScriptableObject
    {
        [SerializeField] private ScreenEffectData[] _effects;

        public Volume GetPrefab(ScreenEffectId id)
        {
            var data = _effects.FirstOrDefault(data => data.Id == id);

            if (data == null)
                throw new Exception($"Screen effect with id {id} not found in catalog");

            return data.Prefab;
        }
    }
}
