using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Game/Inventory/Item")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemName;
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private LootItemView worldPrefab;
        [SerializeField] private GameObject inventoryPrefab;

        [Min(1)][SerializeField] private int width = 1;
        [Min(1)][SerializeField] private int height = 1;
        [Min(1)][SerializeField] private int maxStack = 1;
        [SerializeField] private bool canRotate = true;

        [SerializeField] private float weight = 0.1f;
        [SerializeField] private int value = 1;
        [SerializeField] private ItemCategory category = ItemCategory.Misc;

        private string _cachedId;

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedId))
                {
                    _cachedId = GenerateId(itemName);
                }
                return _cachedId;
            }
        }

        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public LootItemView WorldPrefab => worldPrefab;
        public GameObject InventoryPrefab => inventoryPrefab;
        public int Width => width;
        public int Height => height;
        public int MaxStack => maxStack;
        public bool CanRotate => canRotate;
        public float Weight => weight;
        public int Value => value;
        public ItemCategory Category => category;

        private string GenerateId(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
                return "item_unknown";

            var id = baseName.ToLower()
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace("'", "")
                .Replace("\"", "");

            var chars = id.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _cachedId = null;

            if (!string.IsNullOrEmpty(itemName))
            {
                _cachedId = GenerateId(itemName);
            }
        }
#endif
    }

    public enum ItemCategory
    {
        Misc = 0,
        Resource,
        Weapon,
        Ammo,
        Medical,
        Food,
        Tool,
        Quest,
        Equipment
    }
}