using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable, InlineProperty]
    public sealed class FloatConst : IFloatProvider
    {
        [SerializeField]
        private float value = 1;

        public FloatConst()
        {
        }

        public FloatConst(float value) => this.value = value;

        public float Value => this.value;
    }
}