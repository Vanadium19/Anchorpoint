using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(menuName = "Inventory/Items/Dimensions", fileName = "NewDimensions")]
    public class DimensionsSo : ScriptableObject
    {
        [SerializeField] private int width = 1;
        [SerializeField] private int height = 1;

        public int Width => width;
        public int Height => height;
    }
}
