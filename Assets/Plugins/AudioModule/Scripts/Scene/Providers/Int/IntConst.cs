using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable, InlineProperty]
    public sealed class IntConst : IIntProvider
    {
        [SerializeField]
        private int value = 1;
        
        public int Value => this.value;
    }
}