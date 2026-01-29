using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "Configs/Inventory/ItemConfig")]
    public class ItemConfig : ScriptableObject
    {
        [SerializeField] private ItemName id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;

        [Header("Grid Settings")]
        [Min(1)][SerializeField] private int width = 1;
        [Min(1)][SerializeField] private int height = 1;
        [Min(1)][SerializeField] private int maxStack = 1;

        public ItemName Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public int Width => width;
        public int Height => height;
        public int MaxStack => maxStack;
    }
}