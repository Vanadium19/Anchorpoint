using System;
using UnityEngine;
using Zenject;

namespace InventoryModule.ContextMenu
{
    [Serializable]
    public abstract class ActionConfigBase
    {
        [SerializeField] private string displayName;

        public string DisplayName => displayName;

        public abstract string ActionType { get; }

        public abstract IContextAction Create(DiContainer container, ItemTable item);

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(displayName) ? ActionType : displayName;
        }
    }
}
