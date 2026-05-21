using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable, InlineProperty]
    internal struct EventParameter
    {
        [HorizontalGroup]
        [SerializeField]
        internal string identifier;
        
        [HorizontalGroup]
        [SerializeField]
        internal AudioEventBase value;
    }
}