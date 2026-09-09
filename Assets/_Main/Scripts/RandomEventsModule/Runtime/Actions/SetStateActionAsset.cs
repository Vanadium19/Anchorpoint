using System;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Asset wrapper for <see cref="SetStateAction"/>.</summary>
    [Serializable]
    public class SetStateActionAsset : ZenjectRandomEventActionAsset<SetStateAction>
    {
        [SerializeField] [RandomEventSignalPicker] private string key;
        [SerializeField] private int value;
        [SerializeField] private bool isAdded;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { key, value, isAdded };
    }
}
