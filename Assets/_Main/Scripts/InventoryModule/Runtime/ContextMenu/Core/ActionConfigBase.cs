using System;
using UnityEngine;
using Zenject;

namespace InventoryModule.ContextMenu
{
    [Serializable]
    public abstract class ActionConfigBase
    {
        [SerializeField] private string displayName;

        public abstract string ActionType { get; }

        public abstract IContextAction Create(DiContainer container, ItemTable item);

        public string GetDisplayName() => string.IsNullOrEmpty(displayName) ? null : displayName;
    }
}