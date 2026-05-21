using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable, InlineProperty]
    internal struct IntParameter
    {
        [HorizontalGroup]
        [SerializeField]
        internal string identifier;

        [HorizontalGroup]
        [SerializeField]
        internal int value;
    }
}