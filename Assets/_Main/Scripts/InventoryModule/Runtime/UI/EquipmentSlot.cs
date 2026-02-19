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

        public EquipmentSlotType SlotType => slotType;
        public string SlotName => string.IsNullOrEmpty(slotName) ? slotType.ToString() : slotName;
        public ItemTable EquippedItem { get; private set; }
        public bool IsEquipped => EquippedItem != null;
        public InventoryItem EquippedItemUI { get; private set; }
        public ContainerSection LinkedSection { get; private set; }

        private Vector2 _originalItemSize;
        private Vector2 _originalItemScale;
        private bool _isRestoring;

        private IInventoryManager _inventoryManager;
        private IEquipmentSlotService _slotService;

        [Inject]
        private void Construct(IInventoryManager inventoryManager, IEquipmentSlotService slotService)
        {
            _inventoryManager = inventoryManager;
            _slotService = slotService;
        }

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (slotImage == null) slotImage = GetComponent<Image>();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            if (slotImage != null)
            {
                if (slotIcon != null)
                {
                    slotImage.sprite = slotIcon;
                }
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

        private void RestoreEquippedItem()
        {
            if (_inventoryManager == null) return;

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
            if (_inventoryManager == null) return;

            if (IsEquipped)
            {
                _inventoryManager.SaveEquippedItem(slotType, EquippedItem);
            }
        }

        public bool CanEquip(ItemTable item)
        {
            if (item == null) return false;
            if (IsEquipped) return false;
            if (!item.ItemDataSo.IsEquippable) return false;

            return item.ItemDataSo.EquipmentSlotType == slotType;
        }

        public bool TryEquip(ItemTable item)
        {
            if (!CanEquip(item)) return false;

            if (item.IsRotated)
            {
                item.Rotate();
            }

            EquippedItem = item;

            if (!_isRestoring)
            {
                _inventoryManager?.SaveEquippedItem(slotType, item);
            }

            if (itemPrefab != null)
            {
                var container = ProjectContext.Instance.Container;
                EquippedItemUI = container.InstantiatePrefabForComponent<InventoryItem>(itemPrefab, transform);
                EquippedItemUI.SetItem(item);

                RectTransform itemRect = EquippedItemUI.GetComponent<RectTransform>();
                itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                itemRect.anchoredPosition = Vector2.zero;

                if (useAspectRatioScaling)
                {
                    ScaleItemToFitSlot(itemRect);
                }
            }

            if (item.IsContainer && LinkedSection == null)
            {
                CreateContainerSection(item);
            }

            UpdateVisuals();

            return true;
        }

        private void CreateContainerSection(ItemTable item)
        {
            if (inventoryPanel == null || containerGridPrefab == null)
            {
                return;
            }

            var metadata = item.GetMetadata<ContainerMetadata>();
            if (metadata == null)
            {
                return;
            }

            metadata.InitializeInventories();

            LinkedSection = Instantiate(inventoryPanel.SectionPrefab, inventoryPanel.SectionsContainer);
            inventoryPanel.EnsureSectionHasLayoutElement(LinkedSection);
            LinkedSection.InitializeContainer(item, metadata, containerGridPrefab);

            inventoryPanel.AddContainerSection(LinkedSection);
        }

        private void RemoveContainerSection()
        {
            if (LinkedSection != null)
            {
                if (inventoryPanel != null)
                {
                    inventoryPanel.RemoveContainerSection(LinkedSection);
                }
                LinkedSection.Close();
                LinkedSection = null;
            }
        }

        private void ScaleItemToFitSlot(RectTransform itemRect)
        {
            Vector2 slotSize = rectTransform.sizeDelta;
            Vector2 itemSize = itemRect.sizeDelta;

            _originalItemSize = itemSize;
            _originalItemScale = itemRect.localScale;

            float slotAspect = slotSize.x / slotSize.y;
            float itemAspect = itemSize.x / itemSize.y;

            float scaleFactor;
            if (itemAspect > slotAspect)
            {
                scaleFactor = slotSize.x / itemSize.x;
            }
            else
            {
                scaleFactor = slotSize.y / itemSize.y;
            }

            scaleFactor = Mathf.Min(scaleFactor, 1f);

            itemRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            _originalItemScale = itemRect.localScale;
        }

        public void RestoreItemScale()
        {
            if (EquippedItemUI != null)
            {
                RectTransform itemRect = EquippedItemUI.GetComponent<RectTransform>();
                if (_originalItemScale.sqrMagnitude > 0.001f)
                {
                    itemRect.localScale = _originalItemScale;
                }
            }
        }

        public void Unequip()
        {
            if (EquippedItem == null) return;

            RemoveContainerSection();

            EquippedItem = null;

            if (!_isRestoring)
            {
                _inventoryManager?.RemoveEquippedItem(slotType);
            }

            if (EquippedItemUI != null && EquippedItemUI.gameObject != null && EquippedItemUI.gameObject != gameObject)
            {
                Destroy(EquippedItemUI.gameObject);
                EquippedItemUI = null;
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
            ItemTable item = EquippedItem;

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

        public void ReturnItemToSlot(ItemTable item)
        {
            EquippedItem = item;

            _inventoryManager?.SaveEquippedItem(slotType, item);

            if (item.IsContainer)
            {
                CreateContainerSection(item);
            }

            UpdateVisuals();
        }

        public void ReturnItemToSlotWithUI(ItemTable item, InventoryItem existingUI)
        {
            EquippedItem = item;
            EquippedItemUI = existingUI;

            _inventoryManager?.SaveEquippedItem(slotType, item);

            if (item.IsContainer)
            {
                CreateContainerSection(item);
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (slotImage != null)
            {
                slotImage.color = Color.white;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var draggedItem = eventData.pointerDrag?.GetComponent<AbstractItem>();
            if (draggedItem != null && draggedItem.Item != null)
            {
                if (TryEquip(draggedItem.Item))
                {
                    draggedItem.Item.RemoveItselfFromLocation();

                    if (draggedItem.gameObject != gameObject && (EquippedItemUI == null || draggedItem.gameObject != EquippedItemUI.gameObject))
                    {
                        Destroy(draggedItem.gameObject);
                    }
                }
            }
        }

        public ItemTable GetItem()
        {
            return EquippedItem;
        }
    }
}
