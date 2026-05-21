using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioModule
{
    [Serializable, InlineProperty]
    public sealed class FloatRandom : IFloatProvider
    {
        public float Value => Random.Range(this.minValue, this.maxValue);

        [HorizontalGroup]
        [SerializeField]
        private float minValue = 1;

        [HorizontalGroup]
        [SerializeField]
        private float maxValue = 1;
    }
}