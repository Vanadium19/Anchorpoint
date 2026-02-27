using UnityEngine;

namespace InventoryModule.ContextMenu.Configs
{
    [System.Serializable]
    public class SplitActionConfig : ActionConfigBase
    {
        [SerializeField] private int minStackCount = 2;

        public int MinStackCount => minStackCount;

        public override string ActionType => "Split";
    }
}
