using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;

namespace InventoryModule
{
    public class EquipmentSlot : MonoBehaviour, IDropHandler
    {
        [Header("Slot Settings")]
        [SerializeField] private EquipmentSlotType slotType;
        [SerializeField] private string slotName;
        [SerializeField] private Sprite slotIcon;
        [SerializeField] private bool useAspectRatioScaling = true;

        [Header("UI")]
        [SerializeField] private Image slotImage;
        [SerializeField] private RectTransform rectTransform;

        [Header("Item Prefab")]
        [SerializeField] private InventoryItem itemPrefab;

        [Header("Container Settings")]
        [SerializeField] private InventoryPanel inventoryPanel;
        [SerializeField] private AbstractGrid containerGridPrefab;

        private InventoryItem _equippedItemUI;
        private ContainerSection _linkedSection;

        private bool _isRestoring;

        private IInventoryManager _inventoryManager;
        private IEquipmentSlotService _slotService;
        private DiContainer _diContainer;

        public ItemTable EquippedItem { get; private set; }
        public EquipmentSlotType SlotType => slotType;
        public bool IsEquipped => EquippedItem != null;

        private void Awake()
        {
            InitSlot();
        }

        public void ForceAwake()
        {
            if (_slotService != null && _slotService.GetAllSlots().Contains(this))
                return;

            InitSlot();
        }

        private void InitSlot()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (slotImage == null)
                slotImage = GetComponent<Image>();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            if (slotImage != null)
            {
                if (slotIcon != null)
                    slotImage.sprite = slotIcon;

                slotImage.color = Color.white;
            }

            UpdateVisuals();
            RestoreEquippedItem();
            _slotService?.RegisterSlot(this);
        }

        private void OnDestroy()
        {
            SaveEquippedItem();
            _slotService?.UnregisterSlot(this);
        }


        [Inject]
        private void Construct(IInventoryManager inventoryManager, IEquipmentSlotService slotService, DiContainer diContainer)
        {
            _inventoryManager = inventoryManager;
            _slotService = slotService;
            _diContainer = diContainer;
        }

        public bool CanEquip(ItemTable item)
        {
            if (item == null)
                return false;

            if (IsEquipped)
                return false;

            if (!item.ItemDataSo.IsEquippable)
                return false;

            return item.ItemDataSo.EquipmentSlotType == slotType;
        }

        public bool TryEquip(ItemTable item)
        {
            if (!CanEquip(item))
                return false;

            if (item.IsRotated)
                item.Rotate();

            EquippedItem = item;

            if (!_isRestoring)
                _inventoryManager?.SaveEquippedItem(slotType, item);

            if (itemPrefab != null)
            {
                _equippedItemUI = _diContainer.InstantiatePrefabForComponent<InventoryItem>(itemPrefab, transform);
                _equippedItemUI.SetItem(item);

                var itemRect = _equippedItemUI.GetComponent<RectTransform>();
                itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                itemRect.anchoredPosition = Vector2.zero;

                if (useAspectRatioScaling)
                    ScaleItemToFitSlot(itemRect);
            }

            if (item.IsContainer && _linkedSection == null)
                CreateContainerSection(item);

            UpdateVisuals();

            return true;
        }

        public void Unequip()
        {
            if (EquippedItem == null)
                return;

            RemoveContainerSection();

            EquippedItem = null;

            if (!_isRestoring)
                _inventoryManager?.RemoveEquippedItem(slotType);

            if (_equippedItemUI != null && _equippedItemUI.gameObject != null && _equippedItemUI.gameObject != gameObject)
            {
                Destroy(_equippedItemUI.gameObject);
                _equippedItemUI = null;
            }

            UpdateVisuals();
        }

        public ItemTable ExtractItem(out EquipmentSlot extractedFromSlot)
        {
            if (EquippedItem == null)
            {
                extractedFromSlot = null;
                return null;
            }

            extractedFromSlot = this;
            var item = EquippedItem;

            RemoveContainerSection();

            EquippedItem = null;

            _inventoryManager?.RemoveEquippedItem(slotType);

            UpdateVisuals();
            return item;
        }

        public void OnItemPlacedToInventory()
        {
            RemoveContainerSection();
        }

        public void ReturnItemToSlotWithUI(ItemTable item, InventoryItem existingUI)
        {
            EquippedItem = item;
            _equippedItemUI = existingUI;

            _inventoryManager?.SaveEquippedItem(slotType, item);

            if (item.IsContainer)
                CreateContainerSection(item);

            UpdateVisuals();
        }

        public void OnDrop(PointerEventData eventData)
        {
            var draggedItem = eventData.pointerDrag?.GetComponent<AbstractItem>();

            if (draggedItem == null || draggedItem.Item == null)
                return;

            if (!TryEquip(draggedItem.Item))
                return;

            draggedItem.Item.RemoveItselfFromLocation();

            if (draggedItem.gameObject != gameObject && (_equippedItemUI == null || draggedItem.gameObject != _equippedItemUI.gameObject))
                Destroy(draggedItem.gameObject);
        }

        public ItemTable GetItem() => EquippedItem;

        private void RestoreEquippedItem()
        {
            if (_inventoryManager == null)
                return;

            var savedItem = _inventoryManager.GetEquippedItem(slotType);

            if (savedItem != null && !IsEquipped)
            {
                _isRestoring = true;
                TryEquip(savedItem);
                _isRestoring = false;
            }
        }

        private void SaveEquippedItem()
        {
            if (_inventoryManager == null)
                return;

            if (IsEquipped)
                _inventoryManager.SaveEquippedItem(slotType, EquippedItem);
        }

        private void CreateContainerSection(ItemTable item)
        {
            if (inventoryPanel == null || containerGridPrefab == null)
                return;

            var metadata = item.GetMetadata<ContainerMetadata>();

            if (metadata == null)
                return;

            metadata.InitializeInventories();

            _linkedSection = _diContainer != null
                ? _diContainer.InstantiatePrefabForComponent<ContainerSection>(inventoryPanel.SectionPrefab, inventoryPanel.SectionsContainer)
                : Instantiate(inventoryPanel.SectionPrefab, inventoryPanel.SectionsContainer);
            inventoryPanel.EnsureSectionHasLayoutElement(_linkedSection);
            _linkedSection.InitializeContainer(item, metadata, containerGridPrefab);
            inventoryPanel.AddContainerSection(_linkedSection);
        }

        private void RemoveContainerSection()
        {
            if (_linkedSection == null)
                return;

            if (inventoryPanel != null)
                inventoryPanel.RemoveContainerSection(_linkedSection);

            _linkedSection.Close();
            _linkedSection = null;
        }

        private void ScaleItemToFitSlot(RectTransform itemRect)
        {
            var slotSize = rectTransform.sizeDelta;
            var itemSize = itemRect.sizeDelta;

            var slotAspect = slotSize.x / slotSize.y;
            var itemAspect = itemSize.x / itemSize.y;

            float scaleFactor;

            if (itemAspect > slotAspect)
                scaleFactor = slotSize.x / itemSize.x;
            else
                scaleFactor = slotSize.y / itemSize.y;

            scaleFactor = Mathf.Min(scaleFactor, 1f);

            itemRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }

        private void UpdateVisuals()
        {
            if (slotImage != null)
                slotImage.color = Color.white;
        }
    }
}