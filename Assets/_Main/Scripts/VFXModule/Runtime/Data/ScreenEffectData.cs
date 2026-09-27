using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace VFXModule
{
    [Serializable]
    public class ScreenEffectData
    {
        [HorizontalGroup("vars")] [SerializeField] private ScreenEffectId id;
        [HorizontalGroup("vars")] [SerializeField] private Volume prefab;

        public ScreenEffectId Id => id;
        public Volume Prefab => prefab;
    }
}
