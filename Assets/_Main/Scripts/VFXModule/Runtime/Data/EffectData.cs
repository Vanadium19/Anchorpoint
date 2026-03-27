using System;
using TriInspector;
using UnityEngine;

namespace VFXModule
{
    [Serializable]
    [DeclareHorizontalGroup("vars")]
    public class EffectData
    {
        [Group("vars")] [SerializeField] private EffectId id;
        [Group("vars")] [SerializeField] private EffectView prefab;

        public EffectId Id => id;
        public EffectView Prefab => prefab;
    }
}