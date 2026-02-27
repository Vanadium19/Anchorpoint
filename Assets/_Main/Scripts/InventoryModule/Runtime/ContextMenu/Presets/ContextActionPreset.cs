using System;
using System.Collections.Generic;
using UnityEngine;
using InventoryModule.ContextMenu.Configs;

namespace InventoryModule.ContextMenu.Presets
{
    [CreateAssetMenu(fileName = "NewContextActionPreset", menuName = "Inventory/Context Action Preset")]
    public class ContextActionPreset : ScriptableObject
    {
        [SerializeField] private string presetName;

        [SerializeReference]
        [SerializeField] private List<ActionConfigBase> actionConfigs = new List<ActionConfigBase>();

        public string PresetName => presetName;
        public IReadOnlyList<ActionConfigBase> ActionConfigs => actionConfigs;

        public List<ActionConfigBase> GetSortedConfigs()
        {
            return actionConfigs;
        }
    }
}
