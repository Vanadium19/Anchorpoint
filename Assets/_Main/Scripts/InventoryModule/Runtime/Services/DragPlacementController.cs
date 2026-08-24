using UnityEngine;
using UnityEngine.EventSystems;

namespace InventoryModule
{
    public sealed class DragPlacementController
    {
        private const float PlacementUpdateInterval = 0.033f;

        private readonly IUIInputHandler _uiInputHandler;
        private readonly IGridService _gridService;
        private readonly IInventoryManager _inventoryManager;
        private readonly IEquipmentSlotService _slotService;

        private AbstractGrid _currentTargetGrid;
        private Vector2Int _currentGridPos;
        private bool _currentIsValid;
        private EquipmentSlot _extractedFromSlot;
        private InventoryItem _currentContainerTarget;
        private InventoryItem _stackTargetItem;
        private InventoryDropZone _currentDropZone;
        private GridTable _previousGrid;
        private Position _previousPosition;

        private float _lastPlacementUpdate;

        public bool ShouldDestroyView { get; private set; }
        public bool IsOverDropZone => _currentDropZone != null;

        public DragPlacementController(IUIInputHandler uiInputHandler,
            IGridService gridService,
            IInventoryManager inventoryManager,
            IEquipmentSlotService slotService)
        {
            _uiInputHandler = uiInputHandler;
            _gridService = gridService;
            _inventoryManager = inventoryManager;
            _slotService = slotService;
        }

        public void BeginDrag(InventoryItem view, Transform originalParent)
        {
            _currentTargetGrid = null;
            _currentContainerTarget = null;
            _stackTargetItem = null;
            _currentDropZone = null;
            _currentIsValid = false;

            var equipmentSlot = originalParent?.GetComponent<EquipmentSlot>();

            if (equipmentSlot != null && !equipmentSlot.Equals(null) && equipmentSlot.IsEquipped)
                _slotService.ExtractItem(equipmentSlot, out _extractedFromSlot);
            else
                _extractedFromSlot = null;
        }

        public void UpdatePreview(InventoryItem view)
        {
            var now = Time.unscaledTime;

            if (now - _lastPlacementUpdate < PlacementUpdateInterval)
                return;

            _lastPlacementUpdate = now;

            var newTargetGrid = _uiInputHandler.GetGridUnderMouse(view);

            if (newTargetGrid != _currentTargetGrid)
            {
                if (_currentTargetGrid != null)
                    _currentTargetGrid.HideHighlight();

                _currentTargetGrid = newTargetGrid;
            }

            var newContainerTarget = _uiInputHandler.GetContainerItemUnderMouse(view);

            if (newContainerTarget != _currentContainerTarget)
            {
                ClearContainerHighlight();
                _currentContainerTarget = newContainerTarget;

                if (_currentContainerTarget != null && _currentContainerTarget != view)
                    HighlightContainer(_currentContainerTarget);
            }

            var newStackTarget = _uiInputHandler.GetStackTargetUnderMouse(view, view.Item);
            if (newStackTarget != _stackTargetItem)
            {
                ClearStackHighlight();
                _stackTargetItem = newStackTarget;

                if (_stackTargetItem != null && _stackTargetItem != view)
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

                RectTransformUtility.ScreenPointToLocalPointInRectangle(_currentTargetGrid.GetComponent<RectTransform>(),
                    mousePos,
                    null,
                    out localPos);

                _currentGridPos = _currentTargetGrid.GetGridPosition(localPos);

                var (currentW, currentH) = GetCurrentDimensions(view);

                _currentGridPos.x = Mathf.Clamp(_currentGridPos.x, 0, _currentTargetGrid.GridWidth - currentW);
                _currentGridPos.y = Mathf.Clamp(_currentGridPos.y, 0, _currentTargetGrid.GridHeight - currentH);

                bool fitsInGrid = _currentGridPos.x >= 0 && _currentGridPos.y >= 0
                    && _currentGridPos.x + currentW <= _currentTargetGrid.GridWidth
                    && _currentGridPos.y + currentH <= _currentTargetGrid.GridHeight;

                _currentIsValid = fitsInGrid && _currentTargetGrid.Grid.OverlapCheck(_currentGridPos.x, _currentGridPos.y, currentW, currentH, view.Item);

                _currentTargetGrid.ShowHighlight(_currentGridPos.x, _currentGridPos.y, currentW, currentH, _currentIsValid);
            }
            else
            {
                var dropZone = _uiInputHandler.GetDropZoneUnderMouse(view);

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

        public PlacementOutcome ResolvePlacement(InventoryItem view)
        {
            ShouldDestroyView = false;

            var stackTarget = _stackTargetItem;

            HideAllHighlights();

            if (view == null || view.Item == null)
                return PlacementOutcome.Returned;

            if (stackTarget != null && view.Item.IsStackable)
                if (TryStackToTarget(view, stackTarget))
                    return PlacementOutcome.StackedPartially;

            if (IsOverDropZone)
            {
                DropToWorld(view);
                return ShouldDestroyView ? PlacementOutcome.DroppedToWorld : PlacementOutcome.Returned;
            }

            var targetSlot = _uiInputHandler.GetEquipmentSlotUnderMouse();

            if (targetSlot != null && _slotService.CanEquip(targetSlot, view.Item))
            {
                if (_slotService.TryEquip(targetSlot, view.Item))
                {
                    view.Item.RemoveItselfFromLocation();
                    ShouldDestroyView = true;
                    return PlacementOutcome.Equipped;
                }
            }

            var targetContainerItem = _uiInputHandler.GetContainerItemUnderMouse(view);

            if (targetContainerItem != null && targetContainerItem != view
                && targetContainerItem.Item != null && targetContainerItem.Item.IsContainer)
            {
                var targetGridWindow = _currentTargetGrid?.GetComponentInParent<ContainerWindow>();
                var containerWindow = targetContainerItem.GetComponentInParent<ContainerWindow>();

                if (_currentTargetGrid == null || targetGridWindow == containerWindow)
                {
                    if (view.Item.IsContainer)
                    {
                        var draggedMetadata = view.Item.GetMetadata<ContainerMetadata>();
                        var targetMetadata = targetContainerItem.Item.GetMetadata<ContainerMetadata>();

                        if (draggedMetadata != null && targetMetadata != null
                            && draggedMetadata.IsInsertingInsideYourself(targetMetadata.Inventories.Count > 0 ? targetMetadata.Inventories[0] : null))
                        {
                            ReturnToOriginal(view);
                            return PlacementOutcome.Returned;
                        }
                    }

                    var containerMetadata = targetContainerItem.Item.GetMetadata<ContainerMetadata>();

                    if (containerMetadata != null)
                    {
                        _previousGrid = view.Item.CurrentGrid;
                        _previousPosition = view.Item.Position;

                        view.Item.RemoveItselfFromLocation();
                        GridResponse response = containerMetadata.PlaceItemInInventory(view.Item);

                        if (response == GridResponse.Inserted)
                        {
                            NotifyExtractedSlotPlaced();
                            ShouldDestroyView = true;
                            return PlacementOutcome.InsertedIntoContainer;
                        }
                        else
                        {
                            ReturnToOriginal(view);
                            return PlacementOutcome.Returned;
                        }
                    }
                }
            }

            if (_currentTargetGrid != null && _currentIsValid)
            {
                if (view.Item.IsContainer)
                {
                    var metadata = view.Item.GetMetadata<ContainerMetadata>();

                    if (metadata != null && metadata.IsInsertingInsideYourself(_currentTargetGrid.Grid))
                    {
                        ReturnToOriginal(view);
                        return PlacementOutcome.Returned;
                    }
                }

                if (view.Item.IsRotated != view.IsLocalRotated)
                    view.Item.Rotate();

                _previousGrid = view.Item.CurrentGrid;
                _previousPosition = view.Item.Position;

                var response = _currentTargetGrid.TryPlaceItem(view.Item, _currentGridPos.x, _currentGridPos.y);

                if (response == GridResponse.Inserted)
                {
                    NotifyExtractedSlotPlaced();
                    ShouldDestroyView = true;
                    return PlacementOutcome.PlacedToGrid;
                }
            }

            ReturnToOriginal(view);
            return PlacementOutcome.Returned;
        }

        public void HideAllHighlights()
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

        private void DropToWorld(InventoryItem view)
        {
            if (_currentDropZone == null || view.Item == null || !view.Item.ItemDataSo.IsDropable)
            {
                ReturnToOriginal(view);
                return;
            }

            var success = _currentDropZone.TryDropItem(view.Item);

            if (success)
            {
                view.Item.RemoveItselfFromLocation();

                if (_extractedFromSlot != null)
                {
                    _slotService.Unequip(_extractedFromSlot);
                    _extractedFromSlot = null;
                }

                ShouldDestroyView = true;
            }
            else
            {
                ReturnToOriginal(view);
            }
        }
        private bool TryStackToTarget(InventoryItem view, InventoryItem stackTarget)
        {
            if (stackTarget == null || view.Item == null)
                return false;

            var toAdd = view.Item.StackCount;
            var remaining = stackTarget.Item.TryAddToStack(toAdd);

            if (remaining >= view.Item.StackCount)
                return false;

            if (remaining == 0)
            {
                view.Item.RemoveItselfFromLocation();
                NotifyExtractedSlotPlaced();
                ShouldDestroyView = true;
            }
            else
            {
                view.Item.StackCount = remaining;
                view.RefreshAfterRestore();
                ReturnToOriginal(view);
            }

            return true;
        }

        private void ReturnToOriginal(InventoryItem view)
        {
            view.RefreshAfterRestore();

            if (_extractedFromSlot != null)
            {
                view.ReturnToSlot(_extractedFromSlot);
                _slotService.RestoreToSlot(_extractedFromSlot, view.Item, view);
                _extractedFromSlot = null;
                return;
            }

            if (view.DragParent == null)
            {
                TryAutoEquipToAnySlot(view.Item);
                ShouldDestroyView = true;
                return;
            }

            view.ReparentTo(view.DragParent);

            if (_previousGrid != null && _previousPosition != null)
            {
                _previousGrid.PlaceItem(view.Item, _previousPosition.X, _previousPosition.Y);

                if (view.UpdatePositionInGrid())
                    return;
            }

            if (view.Item.CurrentGrid != null && view.Item.Position != null)
            {
                if (view.UpdatePositionInGrid())
                    return;
            }

            var equipmentSlot = view.DragParent?.GetComponent<EquipmentSlot>();

            if (equipmentSlot != null)
            {
                if (!equipmentSlot.IsEquipped)
                {
                    _slotService.TryEquip(equipmentSlot, view.Item);
                    ShouldDestroyView = true;
                    return;
                }

                if (TryAutoEquipToAnySlot(view.Item))
                {
                    ShouldDestroyView = true;
                    return;
                }

                if (_inventoryManager != null && _inventoryManager.AddExistingItemToInventory(view.Item))
                {
                    ShouldDestroyView = true;
                    return;
                }
            }

            view.SetAnchoredTo(view.DragOriginPosition);
        }

        private void NotifyExtractedSlotPlaced()
        {
            if (_extractedFromSlot != null)
            {
                _extractedFromSlot.OnItemPlacedToInventory();
                _extractedFromSlot = null;
            }
        }

        private bool TryAutoEquipToAnySlot(ItemTable item)
        {
            if (_slotService == null || item == null)
                return false;

            var slots = _slotService.GetAllSlots();

            foreach (var slot in slots)
            {
                if (!_slotService.CanEquip(slot, item) || slot.IsEquipped)
                    continue;

                if (_slotService.TryEquip(slot, item))
                    return true;
            }

            return false;
        }

        private void HighlightContainer(InventoryItem containerItem)
        {
            containerItem?.SetContainerHighlight(true);
        }

        private void ClearContainerHighlight()
        {
            if (_currentContainerTarget != null)
                _currentContainerTarget.SetContainerHighlight(false);

            _currentContainerTarget = null;
        }

        private void HighlightStackTarget(InventoryItem target)
        {
            target?.SetStackHighlight(true);
        }

        private void ClearStackHighlight()
        {
            if (_stackTargetItem != null)
                _stackTargetItem.SetStackHighlight(false);

            _stackTargetItem = null;
        }

        private (int width, int height) GetCurrentDimensions(InventoryItem view)
        {
            if (view.Item == null)
                return (1, 1);

            var originalW = view.Item.ItemDataSo.Width;
            var originalH = view.Item.ItemDataSo.Height;

            if (view.IsLocalRotated)
                return (originalH, originalW);

            return (originalW, originalH);
        }
    }
}
