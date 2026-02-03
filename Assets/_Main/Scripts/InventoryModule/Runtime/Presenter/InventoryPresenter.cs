using InputModule;
using System;
using UnityEngine;
using Zenject;
using ComponentsModule;

namespace InventoryModule
{
    public sealed class InventoryPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly IInventoryService _inventoryService;
        private readonly IItemDatabase _itemDatabase;
        private readonly InventoryView _view;
        private readonly IInputMap _inputMap;
        private readonly IInputService _inputService;
        private readonly IPlayerPositionProvider _playerPosition;
        private readonly InventoryConfig _config;
        private readonly IItemDropService _itemDropService;
        private readonly IInventoryItemPool _itemPool;
        private InventoryItemView _currentlyDraggedItem;
        private float _lastOpenTime;

        public InventoryPresenter(
            IInventoryService inventoryService,
            IItemDatabase itemDatabase,
            InventoryView view,
            IInputMap inputMap,
            IInputService inputService,
            IPlayerPositionProvider playerPosition,
            InventoryConfig config,
            IItemDropService itemDropService,
            IInventoryItemPool itemPool)
        {
            _inventoryService = inventoryService;
            _itemDatabase = itemDatabase;
            _view = view;
            _inputMap = inputMap;
            _inputService = inputService;
            _playerPosition = playerPosition;
            _config = config;
            _itemDropService = itemDropService;
            _itemPool = itemPool;
        }

        public void Initialize()
        {
            _view.Initialize(_itemPool, _itemDatabase, _inputMap);
            _view.GenerateGrid(_inventoryService.Width, _inventoryService.Height);

            _inventoryService.InventoryUpdated += OnInventoryUpdated;
            _view.ItemDropped += OnItemDropped;

            _view.ItemDragStarted += OnItemDragStarted;
            _view.ItemDragEnded += OnItemDragEnded;

            _view.ItemPointerEntered += OnItemPointerEntered;
            _view.ItemPointerExited += OnItemPointerExited;

            OnInventoryUpdated();
            _view.HideTooltip();
        }

        public void Dispose()
        {
            _inventoryService.InventoryUpdated -= OnInventoryUpdated;
            _view.ItemDropped -= OnItemDropped;

            _view.ItemDragStarted -= OnItemDragStarted;
            _view.ItemDragEnded -= OnItemDragEnded;

            _view.ItemPointerEntered -= OnItemPointerEntered;
            _view.ItemPointerExited -= OnItemPointerExited;
        }

        private void OnItemDragStarted(InventoryItemView view) => _currentlyDraggedItem = view;

        private void OnItemDragEnded(InventoryItemView view) => _currentlyDraggedItem = null;

        public void Tick()
        {
            if (_inputMap.IsInventoryPressed)
            {
                bool newState = !_view.IsVisible;

                if (newState)
                {
                    _lastOpenTime = Time.time;
                }
                else
                {
                    _view.HideTooltip();
                }

                _view.Toggle(newState);
                _inputService.SetUIMode(newState);
            }

            if (_view.IsVisible && _inputMap.IsRotatePressed && _currentlyDraggedItem != null)
            {
                _currentlyDraggedItem.Rotate();
            }
        }

        private void OnItemDropped(InventoryItemView itemView, Vector2Int newPosition, Vector2 screenPosition)
        {
            var item = itemView.Item;
            if (item == null) return;

            if (!_view.IsMouseOverGrid(screenPosition))
            {
                HandleItemDiscard(itemView);
                return;
            }

            if (itemView.IsSplitting)
            {
                _inventoryService.SplitItem(item, newPosition, itemView.LocalIsRotated);
            }
            else
            {
                _inventoryService.MoveItem(item, newPosition, itemView.LocalIsRotated);
            }
            OnInventoryUpdated();
        }

        private void OnItemPointerEntered(InventoryItemView itemView)
        {
            if (!_view.IsVisible || (Time.time - _lastOpenTime) < 0.1f)
                return;

            if (_itemDatabase.TryGetItem(itemView.Item.Id, out var itemDef))
            {
                _view.ShowTooltip(itemDef.ItemName);
            }
        }

        private void OnItemPointerExited(InventoryItemView itemView)
        {
            _view.HideTooltip();
        }

        private void HandleItemDiscard(InventoryItemView itemView)
        {
            var item = itemView.Item;
            if (item == null) return;
            if (!_itemDatabase.TryGetItem(item.Id, out var itemDef)) return;
            int amountToDiscard;
            if (itemView.IsSplitting)
            {
                amountToDiscard = item.Amount / 2;
                item.AddAmount(-amountToDiscard);
            }
            else
            {
                amountToDiscard = item.Amount;
                var result = _inventoryService.RemoveItem(item);

                if (!result.Success)
                {
                    return;
                }
            }
            _itemDropService.SpawnItemInWorld(itemDef, amountToDiscard, _playerPosition, _config);
            OnInventoryUpdated();
        }

        private void OnInventoryUpdated()
        {
            if (_view == null) return;
            var items = _inventoryService.GetAllItems();
            _view.Render(items);
        }
    }
}