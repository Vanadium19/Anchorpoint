using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Zenject;
using InventoryModule.ContextMenu;
using InventoryModule.ContextMenu.UI;

namespace InventoryModule
{
    public class InventoryItem : AbstractItem
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private ContainerWindow containerWindowPrefab;
        [SerializeField] private ContextMenuPresenter contextMenuPrefab;

        private AbstractGrid _currentTargetGrid;
        private Vector2Int _currentGridPos;
        private bool _currentIsValid;
        private EquipmentSlot _extractedFromSlot;
        private InventoryItem _currentContainerTarget;
        private InventoryItem _stackTargetItem;
        private Color _originalItemColor = Color.white;
        private InventoryDropZone _currentDropZone;

        private GridTable _previousGrid;
        private Position _previousPosition;

        private float _lastPlacementUpdate;
        private const float PlacementUpdateInterval = 0.033f;

        private IInventoryManager _inventoryManager;
        private IEquipmentSlotService _slotService;
        private IGridService _gridService;
        private IUIInputHandler _uiInputHandler;
        private IContextActionService _contextActionService;
        private IContainerWindowService _windowService;
        private IContextMenuStateService _menuStateService;
        private ContextMenuPresenter _contextMenu;

        [Inject]
        private void Construct(
            IInventoryManager inventoryManager,
            IEquipmentSlotService slotService,
            IGridService gridService,
            IUIInputHandler uiInputHandler,
            IContextActionService contextActionService,
            IContainerWindowService windowService,
            IContextMenuStateService menuStateService)
        {
            _inventoryManager = inventoryManager;
            _slotService = slotService;
            _gridService = gridService;
            _uiInputHandler = uiInputHandler;
            _contextActionService = contextActionService;
            _windowService = windowService;
            _menuStateService = menuStateService;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (Item == null)
                return;

            if (eventData.button == PointerEventData.InputButton.Right)
                ShowContextMenu(eventData.position);
            else if (eventData.clickCount == 2 && Item.IsContainer)
                OpenContainerWindow();
        }

        private void ShowContextMenu(Vector2 screenPosition)
        {
            if (Item == null)
                return;

            if (contextMenuPrefab == null)
                return;

            var contextService = _contextActionService;

            if (contextService == null || !contextService.IsInitialized)
                return;

            if (_contextMenu == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();

                if (canvas == null)
                    return;

                _contextMenu = UnityEngine.Object.Instantiate(contextMenuPrefab, canvas.transform);
            }

            _contextMenu.Initialize(contextService, _menuStateService);
            _contextMenu.ShowForItem(Item, screenPosition);
        }

        private void OpenContainerWindow()
        {
            var metadata = Item.GetMetadata<ContainerMetadata>();

            if (metadata == null || containerWindowPrefab == null)
                return;

            if (_windowService.IsContainerOpen(Item))
                return;

            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
                return;

            ContainerWindow window = UnityEngine.Object.Instantiate(containerWindowPrefab, canvas.transform);
            window.transform.SetAsLastSibling();
            window.Initialize(Item, metadata, containerWindowPrefab.GridPrefab, _windowService);
        }

        private Vector2 _stackTextOriginalPos;
        private bool _stackTextPosInitialized;

        protected override void Awake()
        {
            base.Awake();

            if (stackText != null && !_stackTextPosInitialized)
            {
                _stackTextOriginalPos = stackText.rectTransform.anchoredPosition;
                _stackTextPosInitialized = true;
            }
        }

        private void OnDestroy()
        {
            if (_contextMenu != null)
            {
                Destroy(_contextMenu.gameObject);
                _contextMenu = null;
            }
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();

            if (stackText != null && Item != null)
            {
                if (Item.IsStackable)
                {
                    stackText.text = Item.StackCount.ToString();
                    stackText.gameObject.SetActive(true);
                    UpdateStackTextTransform();
                }
                else
                    stackText.gameObject.SetActive(false);
            }
        }

        private void UpdateStackTextTransform()
        {
            if (stackText == null)
                return;

            var textRT = stackText.rectTransform;

            if (_localIsRotated)
            {
                float offset = 50f * Item.ItemDataSo.Height;
                textRT.localRotation = Quaternion.Euler(0, 0, 90f);
                textRT.anchoredPosition = _stackTextOriginalPos + new Vector2(0, offset);
            }
            else
            {
                textRT.localRotation = Quaternion.identity;
                textRT.anchoredPosition = _stackTextOriginalPos;
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (IsDragging)
                return;

            if (_contextMenu != null)
            {
                Destroy(_contextMenu.gameObject);
                _contextMenu = null;
            }

            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

            base.OnBeginDrag(eventData);

            var equipmentSlot = originalParent?.GetComponent<EquipmentSlot>();

            if (equipmentSlot != null && !equipmentSlot.Equals(null) && equipmentSlot.IsEquipped)
                equipmentSlot.ExtractItem(out _extractedFromSlot);
            else
                _extractedFromSlot = null;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging)
                return;

            base.OnDrag(eventData);
            UpdatePlacementPreview();
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
        }

        private void UpdatePlacementPreview()
        {
            float now = Time.unscaledTime;

            if (now - _lastPlacementUpdate < PlacementUpdateInterval)
                return;

            _lastPlacementUpdate = now;

            var newTargetGrid = _uiInputHandler.GetGridUnderMouse(this);

            if (newTargetGrid != _currentTargetGrid)
            {
                if (_currentTargetGrid != null)
                    _currentTargetGrid.HideHighlight();

                _currentTargetGrid = newTargetGrid;
            }

            var newContainerTarget = _uiInputHandler.GetContainerItemUnderMouse(this);

            if (newContainerTarget != _currentContainerTarget)
            {
                ClearContainerHighlight();
                _currentContainerTarget = newContainerTarget;

                if (_currentContainerTarget != null && _currentContainerTarget != this)
                    HighlightContainer(_currentContainerTarget);
            }

            var newStackTarget = _uiInputHandler.GetStackTargetUnderMouse(this, Item);

            if (newStackTarget != _stackTargetItem)
            {
                ClearStackHighlight();
                _stackTargetItem = newStackTarget;

                if (_stackTargetItem != null && _stackTargetItem != this)
                    HighlightStackTarget(_stackTargetItem);
            }

            if (_currentTargetGrid != null)
            {
                if (_currentDropZone != null)
                {
                    _currentDropZone.ShowHighlight(false);
                    _currentDropZone = null;
                }

                Vector2 mousePos = Input.mousePosition;
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _currentTargetGrid.GetComponent<RectTransform>(),
                    mousePos, null, out localPos);

                _currentGridPos = _currentTargetGrid.GetGridPosition(localPos);

                var (currentW, currentH) = GetCurrentDimensions();

                _currentGridPos.x = Mathf.Clamp(_currentGridPos.x, 0, _currentTargetGrid.GridWidth - currentW);
                _currentGridPos.y = Mathf.Clamp(_currentGridPos.y, 0, _currentTargetGrid.GridHeight - currentH);

                bool fitsInGrid = _currentGridPos.x >= 0 && _currentGridPos.y >= 0 &&
                                  _currentGridPos.x + currentW <= _currentTargetGrid.GridWidth &&
                                  _currentGridPos.y + currentH <= _currentTargetGrid.GridHeight;

                _currentIsValid = fitsInGrid && _currentTargetGrid.Grid.OverlapCheck(
                    _currentGridPos.x, _currentGridPos.y, currentW, currentH, Item);

                _currentTargetGrid.ShowHighlight(_currentGridPos.x, _currentGridPos.y,
                    currentW, currentH, _currentIsValid);
            }
            else
            {
                var dropZone = _uiInputHandler.GetDropZoneUnderMouse(this);

                if (_currentDropZone != dropZone)
                {
                    if (_currentDropZone != null)
                        _currentDropZone.ShowHighlight(false);

                    _currentDropZone = dropZone;

                    if (_currentDropZone != null)
                        _currentDropZone.ShowHighlight(true);
                }

                _currentTargetGrid?.HideHighlight();
                ClearContainerHighlight();
                _currentIsValid = false;
            }
        }

        private (int width, int height) GetCurrentDimensions()
        {
            if (Item == null) 
                return (1, 1);

            int originalW = Item.ItemDataSo.Width;
            int originalH = Item.ItemDataSo.Height;

            if (_localIsRotated)
                return (originalH, originalW);

            return (originalW, originalH);
        }

        protected override void TryPlaceItem()
        {
            HideAllHighlights();

            if (_stackTargetItem != null && Item != null && Item.IsStackable)
                if (TryStackToTarget()) return;

            if (IsOverDropZone())
            {
                DropToWorld();
                return;
            }

            EquipmentSlot targetSlot = _uiInputHandler.GetEquipmentSlotUnderMouse();

            if (targetSlot != null && targetSlot.CanEquip(Item))
            {
                if (targetSlot.TryEquip(Item))
                {
                    Item.RemoveItselfFromLocation();
                    Destroy(gameObject);
                    return;
                }
            }

            InventoryItem targetContainerItem = _uiInputHandler.GetContainerItemUnderMouse(this);
            if (targetContainerItem != null && targetContainerItem != this && targetContainerItem.Item != null && targetContainerItem.Item.IsContainer)
            {
                var targetGridWindow = _currentTargetGrid?.GetComponentInParent<ContainerWindow>();
                var containerWindow = targetContainerItem.GetComponentInParent<ContainerWindow>();

                if (_currentTargetGrid == null || targetGridWindow == containerWindow)
                {
                    if (Item.IsContainer)
                    {
                        var draggedMetadata = Item.GetMetadata<ContainerMetadata>();
                        var targetMetadata = targetContainerItem.Item.GetMetadata<ContainerMetadata>();

                        if (draggedMetadata != null && targetMetadata != null && draggedMetadata.IsInsertingInsideYourself(targetMetadata.Inventories.Count > 0 ? targetMetadata.Inventories[0] : null))
                        {
                            ReturnToOriginal();
                            return;
                        }
                    }

                    var containerMetadata = targetContainerItem.Item.GetMetadata<ContainerMetadata>();

                    if (containerMetadata != null)
                    {
                        _previousGrid = Item.CurrentGrid;
                        _previousPosition = Item.Position;

                        Item.RemoveItselfFromLocation();
                        GridResponse response = containerMetadata.PlaceItemInInventory(Item);

                        if (response == GridResponse.Inserted)
                        {
                            if (_extractedFromSlot != null)
                            {
                                _extractedFromSlot.OnItemPlacedToInventory();
                                _extractedFromSlot = null;
                            }

                            Destroy(gameObject);
                            return;
                        }
                        else
                        {
                            ReturnToOriginal();
                            return;
                        }
                    }
                }
            }

            if (_currentTargetGrid != null && _currentIsValid)
            {
                if (Item.IsContainer)
                {
                    var metadata = Item.GetMetadata<ContainerMetadata>();

                    if (metadata != null && metadata.IsInsertingInsideYourself(_currentTargetGrid.Grid))
                    {
                        ReturnToOriginal();
                        return;
                    }
                }

                if (Item != null)
                    Item.UIUpdated -= UpdateUI;

                if (Item.IsRotated != _localIsRotated)
                    Item.Rotate();

                transform.SetParent(_currentTargetGrid.transform, false);

                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                _previousGrid = Item.CurrentGrid;
                _previousPosition = Item.Position;

                var response = _currentTargetGrid.TryPlaceItem(Item, _currentGridPos.x, _currentGridPos.y);

                if (response == GridResponse.Inserted)
                {
                    if (_extractedFromSlot != null)
                    {
                        _extractedFromSlot.OnItemPlacedToInventory();
                        _extractedFromSlot = null;
                    }

                    Destroy(gameObject);
                    return;
                }
                else
                {
                    ReturnToOriginal();
                    return;
                }
            }
            else
            {
                ReturnToOriginal();
                return;
            }
        }

        public bool PlaceInInventory()
        {
            if (_inventoryManager == null) 
                return false;

            bool success = _inventoryManager.AddExistingItemToInventory(Item);

            if (success)
            {
                if (_extractedFromSlot != null)
                {
                    _extractedFromSlot.OnItemPlacedToInventory();
                    _extractedFromSlot = null;
                }

                Destroy(gameObject);
                return true;
            }
            else
            {
                if (_extractedFromSlot != null)
                {
                    _extractedFromSlot.ReturnItemToSlotWithUI(Item, this);
                    _extractedFromSlot = null;
                    return false;
                }
            }

            return false;
        }

        private void ReturnToOriginal()
        {
            _localIsRotated = Item.IsRotated;
            UpdateUI();

            if (_extractedFromSlot != null)
            {
                transform.SetParent(_extractedFromSlot.transform, false);

                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;

                _extractedFromSlot.ReturnItemToSlotWithUI(Item, this);
                _extractedFromSlot = null;
                return;
            }

            if (originalParent == null)
            {
                var slots = _slotService.GetAllSlots();

                foreach (var slot in slots)
                {
                    if (slot.CanEquip(Item))
                    {
                        slot.TryEquip(Item);
                        Destroy(gameObject);
                        return;
                    }
                }

                if (_inventoryManager != null && _inventoryManager.AddExistingItemToInventory(Item))
                {
                    Destroy(gameObject);
                    return;
                }

                Destroy(gameObject);
                return;
            }

            transform.SetParent(originalParent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            if (_previousGrid != null && _previousPosition != null)
            {
                _previousGrid.PlaceItem(Item, _previousPosition.X, _previousPosition.Y);

                var grid = originalParent?.GetComponent<AbstractGrid>();

                if (grid != null)
                {
                    grid.UpdateItemPosition(this);
                    return;
                }
            }

            if (Item.CurrentGrid != null && Item.Position != null)
            {
                var grid = originalParent?.GetComponent<AbstractGrid>();

                if (grid != null)
                {
                    grid.UpdateItemPosition(this);
                    return;
                }
            }

            var equipmentSlot = originalParent?.GetComponent<EquipmentSlot>();

            if (equipmentSlot != null)
            {
                if (!equipmentSlot.IsEquipped)
                {
                    equipmentSlot.TryEquip(Item);
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    var slots = _slotService.GetAllSlots();
                    foreach (var slot in slots)
                    {
                        if (slot.CanEquip(Item) && !slot.IsEquipped)
                        {
                            slot.TryEquip(Item);
                            Destroy(gameObject);
                            return;
                        }
                    }

                    if (_inventoryManager != null && _inventoryManager.AddExistingItemToInventory(Item))
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }

            rectTransform.anchoredPosition = originalPosition;
        }

        protected override void UpdateGridHighlight()
        {
            UpdatePlacementPreview();
        }

        protected override void HideAllHighlights()
        {
            if (_gridService != null)
            {
                var grids = _gridService.GetAllGrids();
                foreach (var grid in grids) grid.HideHighlight();
            }

            ClearContainerHighlight();
            ClearStackHighlight();

            if (_currentDropZone != null)
                _currentDropZone.ShowHighlight(false);
        }

        private void HighlightContainer(InventoryItem containerItem)
        {
            if (containerItem?.iconImage == null) 
                return;

            _originalItemColor = containerItem.iconImage.color;
            Color highlightColor = _originalItemColor;
            highlightColor.a = 0.7f;
            highlightColor.g = Mathf.Min(1f, highlightColor.g + 0.3f);
            containerItem.iconImage.color = highlightColor;
        }

        private void ClearContainerHighlight()
        {
            if (_currentContainerTarget != null && _currentContainerTarget.iconImage != null)
            {
                _currentContainerTarget.iconImage.color = _originalItemColor;
                _currentContainerTarget = null;
            }
        }

        private void HighlightStackTarget(InventoryItem target)
        {
            if (target?.iconImage == null) 
                return;

            _originalItemColor = target.iconImage.color;
            Color highlightColor = new Color(0.5f, 1f, 0.5f, 1f);
            target.iconImage.color = highlightColor;
        }

        private void ClearStackHighlight()
        {
            if (_stackTargetItem != null && _stackTargetItem.iconImage != null)
                _stackTargetItem.iconImage.color = _originalItemColor;
        }

        private bool TryStackToTarget()
        {
            if (_stackTargetItem == null || Item == null) 
                return false;

            int toAdd = Item.StackCount;
            int remaining = _stackTargetItem.Item.TryAddToStack(toAdd);

            if (remaining < Item.StackCount)
            {
                if (remaining == 0)
                {
                    Item.RemoveItselfFromLocation();

                    if (_extractedFromSlot != null)
                    {
                        _extractedFromSlot.OnItemPlacedToInventory();
                        _extractedFromSlot = null;
                    }

                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Item.StackCount = remaining;
                    UpdateUI();
                    ReturnToOriginal();
                    return true;
                }
            }

            return false;
        }

        private bool IsOverDropZone()
        {
            return _currentDropZone != null;
        }

        private void DropToWorld()
        {
            if (_currentDropZone == null || Item == null)
            {
                ReturnToOriginal();
                return;
            }

            if (!Item.ItemDataSo.IsDropable)
            {
                ReturnToOriginal();
                return;
            }

            bool success = _currentDropZone.TryDropItem(Item);

            if (success)
            {
                Item.RemoveItselfFromLocation();

                if (_extractedFromSlot != null)
                {
                    _extractedFromSlot.Unequip();
                    _extractedFromSlot = null;
                }

                Destroy(gameObject);
            }
            else
            {
                ReturnToOriginal();
            }
        }
    }
}
