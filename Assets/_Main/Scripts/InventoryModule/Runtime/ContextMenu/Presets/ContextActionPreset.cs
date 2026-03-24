using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule.ContextMenu.Presets
{
    [CreateAssetMenu(fileName = "NewContextActionPreset", menuName = "Inventory/Context Action Preset")]
    public class ContextActionPreset : ScriptableObject
    {
        [SerializeField] private string presetName;

        [SerializeReference]
        [SerializeField] private List<ActionConfigBase> actionConfigs = new();

        public List<ActionConfigBase> GetSortedConfigs() => actionConfigs;
    }
}
