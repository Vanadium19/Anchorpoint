using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(fileName = "InventoryConfig", menuName = "Configs/Inventory/InventoryConfig")]
    public class InventoryConfig : ScriptableObject
    {
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 5;

        public int Width => width;
        public int Height => height;
    }
}