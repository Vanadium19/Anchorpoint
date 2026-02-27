using System;
using UnityEngine;

namespace InventoryModule.ContextMenu
{
    [Serializable]
    public abstract class ActionConfigBase
    {
        [SerializeField] private string displayName;

        public string DisplayName => displayName;

        public abstract string ActionType { get; }

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(displayName) ? ActionType : displayName;
        }
    }
}
