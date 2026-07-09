using UnityEngine;
using InventoryModule.ContextMenu.Presets;

namespace InventoryModule
{
    [CreateAssetMenu(menuName = "Inventory/Items/Item Data", fileName = "NewItemData")]
    public class ItemDataSo : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string displayName = "Item";
        [SerializeField, TextArea] private string description = "";
        [SerializeField] private Sprite icon;

        [Header("Dimensions")]
        [SerializeField] private DimensionsSo dimensionsSo;

        [Header("Equipment Type")]
        [SerializeField] private EquipmentSlotType equipmentSlotType = EquipmentSlotType.None;
        [SerializeField] private bool isEquippable = false;

        [Header("Pickup")]
        [SerializeField] private ItemPickupBehavior pickupBehavior = ItemPickupBehavior.StoreInInventory;

        [Header("Settings")]
        [SerializeField] private bool isStackable = false;
        [SerializeField] private int maxStackSize = 1;
        [SerializeField] private bool isContainer = false;
        [SerializeField] private ContainerGridsData containerGrids;
        [SerializeField] private bool canRotate = true;

        [Header("Durability")]
        [SerializeField] private bool hasDurability;
        [SerializeField] private int maxDurability = 1;
        [SerializeField] private bool showDurability = true;

        [Header("Effect")]
        [SerializeField] private UseEffectDataSo effectData;

        [Header("World Drop")]
        [SerializeField] private bool isDropable = true;
        [SerializeField] private LootItemView worldPrefab;

        [Header("Context Menu")]
        [SerializeField] private ContextActionPreset contextActionPreset;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public EquipmentSlotType EquipmentSlotType => equipmentSlotType;
        public bool IsEquippable => isEquippable;
        public ItemPickupBehavior PickupBehavior => pickupBehavior;
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;
        public bool IsContainer => isContainer;
        public ContainerGridsData ContainerGrids => containerGrids;
        public bool CanRotate => canRotate;
        public bool IsDropable => isDropable;
        public LootItemView WorldPrefab => worldPrefab;
        public ContextActionPreset ContextActionPreset => contextActionPreset;
        public bool HasDurability => hasDurability;
        public int MaxDurability => maxDurability;
        public bool ShowDurability => showDurability;
        public UseEffectDataSo EffectData => effectData;

        public int Width => dimensionsSo != null ? dimensionsSo.Width : 1;
        public int Height => dimensionsSo != null ? dimensionsSo.Height : 1;
    }
}
