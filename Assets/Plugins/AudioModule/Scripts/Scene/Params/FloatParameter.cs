using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable, InlineProperty]
    internal struct FloatParameter
    {
        [HorizontalGroup]
        [SerializeField]
        internal string identifier;

        [HorizontalGroup]
        [SerializeField]
        internal float value;
    }
}