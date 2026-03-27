using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VFXModule
{
    [Serializable]
    public class EffectData
    {
        [HorizontalGroup("vars")] [SerializeField] private EffectId id;
        [HorizontalGroup("vars")] [SerializeField] private EffectView prefab;

        public EffectId Id => id;
        public EffectView Prefab => prefab;
    }
}