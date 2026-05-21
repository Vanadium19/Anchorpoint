using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioModule
{
    [Serializable, InlineProperty]
    public sealed class IntRandom : IIntProvider
    {
        public int Value => Random.Range(this.minValue, this.maxValue);

        [HorizontalGroup]
        [SerializeField]
        private int minValue = 1;

        [HorizontalGroup]
        [SerializeField]
        private int maxValue = 1;
    }
}