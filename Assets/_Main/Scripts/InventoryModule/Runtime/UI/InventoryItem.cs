using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Zenject;

namespace InventoryModule
{
    public class InventoryItem : AbstractItem
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private ContainerWindow containerWindowPrefab;

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

        [Inject]
        private void Construct(IInventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (eventData.clickCount == 2 && Item != null && Item.IsContainer)
            {
                OpenContainerWindow();
            }
        }

        private void OpenContainerWindow()
        {
            var metadata = Item.GetMetadata<ContainerMetadata>();
            if (metadata == null || containerWindowPrefab == null) return;

            if (ContainerWindow.IsContainerOpen(Item)) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            ContainerWindow window = Instantiate(containerWindowPrefab, canvas.transform);
            window.Initialize(Item, metadata, containerWindowPrefab.GridPrefab);
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
                {
                    stackText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateStackTextTransform()
        {
            if (stackText == null) return;

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
            if (IsDragging) return;

            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

            base.OnBeginDrag(eventData);

            var equipmentSlot = originalParent?.GetComponent<EquipmentSlot>();
            if (equipmentSlot != null && !equipmentSlot.Equals(null) && equipmentSlot.IsEquipped)
            {
                equipmentSlot.ExtractItem(out _extractedFromSlot);
            }
            else
            {
                _extractedFromSlot = null;
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;

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
            if (now - _lastPlacementUpdate < PlacementUpdateInterval) return;

            _lastPlacementUpdate = now;

            var newTargetGrid = GetGridUnderMouse();

            if (newTargetGrid != _currentTargetGrid)
            {
                if (_currentTargetGrid != null)
                    _currentTargetGrid.HideHighlight();
                _currentTargetGrid = newTargetGrid;
            }

            var newContainerTarget = GetContainerItemUnderMouse();
            if (newContainerTarget != _currentContainerTarget)
            {
                ClearContainerHighlight();
                _currentContainerTarget = newContainerTarget;
                if (_currentContainerTarget != null && _currentContainerTarget != this)
                {
                    HighlightContainer(_currentContainerTarget);
                }
            }

            var newStackTarget = GetStackTargetUnderMouse();
            if (newStackTarget != _stackTargetItem)
            {
                ClearStackHighlight();
                _stackTargetItem = newStackTarget;
                if (_stackTargetItem != null && _stackTargetItem != this)
                {
                    HighlightStackTarget(_stackTargetItem);
                }
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
                var dropZone = GetDropZoneUnderMouse();

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
            if (Item == null) return (1, 1);

            int originalW = Item.ItemDataSo.Width;
            int originalH = Item.ItemDataSo.Height;

            if (_localIsRotated)
            {
                return (originalH, originalW);
            }

            return (originalW, originalH);
        }

        protected override void TryPlaceItem()
        {
            HideAllHighlights();

            if (_stackTargetItem != null && Item != null && Item.IsStackable)
            {
                if (TryStackToTarget()) return;
            }

            if (IsOverDropZone())
            {
                DropToWorld();
                return;
            }

            EquipmentSlot targetSlot = GetEquipmentSlotUnderMouse();
            if (targetSlot != null && targetSlot.CanEquip(Item))
            {
                if (targetSlot.TryEquip(Item))
                {
                    Item.RemoveItselfFromLocation();
                    Destroy(gameObject);
                    return;
                }
            }

            InventoryItem targetContainerItem = GetContainerItemUnderMouse();
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
                {
                    Item.Rotate();
                }

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
            if (_inventoryManager == null) return false;

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

        private EquipmentSlot GetEquipmentSlotUnderMouse()
        {
            Vector2 mousePos = Input.mousePosition;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                var slot = result.gameObject.GetComponent<EquipmentSlot>();
                if (slot != null) return slot;
            }

            return null;
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
                var slots = FindObjectsOfType<EquipmentSlot>();
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
                    var slots = FindObjectsOfType<EquipmentSlot>();
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

        private AbstractGrid GetGridUnderMouse()
        {
            Vector2 mousePos = Input.mousePosition;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    continue;

                var grid = result.gameObject.GetComponentInParent<AbstractGrid>();
                if (grid != null) return grid;
            }

            return null;
        }

        private InventoryItem GetContainerItemUnderMouse()
        {
            Vector2 mousePos = Input.mousePosition;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    continue;

                var itemUI = result.gameObject.GetComponentInParent<InventoryItem>();
                if (itemUI != null && itemUI.Item != null && itemUI.Item.IsContainer)
                    return itemUI;
            }

            return null;
        }

        protected override void UpdateGridHighlight()
        {
            UpdatePlacementPreview();
        }

        protected override void HideAllHighlights()
        {
            var grids = FindObjectsOfType<AbstractGrid>();
            foreach (var grid in grids) grid.HideHighlight();
            ClearContainerHighlight();
            ClearStackHighlight();

            if (_currentDropZone != null)
            {
                _currentDropZone.ShowHighlight(false);
            }
        }

        private void HighlightContainer(InventoryItem containerItem)
        {
            if (containerItem?.iconImage == null) return;

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

        private InventoryDropZone GetDropZoneUnderMouse()
        {
            Vector2 mousePos = Input.mousePosition;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    continue;

                var dropZone = result.gameObject.GetComponentInParent<InventoryDropZone>();
                if (dropZone != null) return dropZone;
            }

            return null;
        }

        private InventoryItem GetStackTargetUnderMouse()
        {
            if (Item == null || !Item.IsStackable) return null;

            Vector2 mousePos = Input.mousePosition;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    continue;

                var itemUI = result.gameObject.GetComponentInParent<InventoryItem>();
                if (itemUI != null && itemUI != this && itemUI.Item != null)
                {
                    if (itemUI.Item.ItemDataSo == Item.ItemDataSo && itemUI.Item.IsStackable)
                    {
                        if (itemUI.Item.StackCount < itemUI.Item.MaxStack)
                        {
                            return itemUI;
                        }
                    }
                }
            }

            return null;
        }

        private void HighlightStackTarget(InventoryItem target)
        {
            if (target?.iconImage == null) return;

            _originalItemColor = target.iconImage.color;
            Color highlightColor = new Color(0.5f, 1f, 0.5f, 1f);
            target.iconImage.color = highlightColor;
        }

        private void ClearStackHighlight()
        {
            if (_stackTargetItem != null && _stackTargetItem.iconImage != null)
            {
                _stackTargetItem.iconImage.color = _originalItemColor;
            }
        }

        private bool TryStackToTarget()
        {
            if (_stackTargetItem == null || Item == null) return false;

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
                if (_extractedFromSlot != null)
                {
                    _extractedFromSlot.OnItemPlacedToInventory();
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
