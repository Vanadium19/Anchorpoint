using System;
using System.Threading;
using InputModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public sealed class InventoryPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly IInventoryService _inventoryService;
        private readonly IItemProvider _itemProvider;
        private readonly InventoryView _view;
        private readonly IInputMap _inputMap;
        private readonly IInputService _inputService;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private InventoryItemView _currentlyDraggedItem;

        public InventoryPresenter(
            IInventoryService inventoryService,
            IItemProvider itemProvider,
            InventoryView view,
            IInputMap inputMap,
            IInputService inputService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _itemProvider = itemProvider ?? throw new ArgumentNullException(nameof(itemProvider));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inputMap = inputMap ?? throw new ArgumentNullException(nameof(inputMap));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            _view.Initialize(_itemProvider, _inputMap);
            _view.GenerateGrid(_inventoryService.Width, _inventoryService.Height);

            _inventoryService.InventoryUpdated += OnInventoryUpdated;
            _view.ItemDropped += OnItemDropped;
            _view.ItemRemoved += OnItemRemoved;

            _view.ItemDragStarted += OnItemDragStarted;
            _view.ItemDragEnded += OnItemDragEnded;

            OnInventoryUpdated();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            _inventoryService.InventoryUpdated -= OnInventoryUpdated;
            _view.ItemDropped -= OnItemDropped;
            _view.ItemRemoved -= OnItemRemoved;

            _view.ItemDragStarted -= OnItemDragStarted;
            _view.ItemDragEnded -= OnItemDragEnded;
        }

        private void OnItemDragStarted(InventoryItemView itemView)
        {
            _currentlyDraggedItem = itemView;
        }

        private void OnItemDragEnded(InventoryItemView itemView)
        {
            _currentlyDraggedItem = null;
        }
        public void Tick()
        {
            if (_inputMap.IsInventoryPressed)
            {
                bool newState = !_view.IsVisible;
                _view.Toggle(newState);
                _inputService.SetUIMode(newState);
            }

            if (_view.IsVisible && _inputMap.IsRotatePressed && _currentlyDraggedItem != null)
            {
                _currentlyDraggedItem.Rotate();
            }
        }

        private void OnInventoryUpdated()
        {
            var items = _inventoryService.GetAllItems();
            _view.Render(items, _itemProvider);
        }

        private async void OnItemDropped(InventoryItemView itemView, Vector2Int newPosition)
        {
            var item = itemView.Item;
            if (item == null)
                return;

            InventoryOperationResult result;

            if (itemView.IsSplitting)
            {
                result = await _inventoryService.SplitItemAsync(
                    item,
                    newPosition,
                    itemView.LocalIsRotated,
                    _cancellationTokenSource.Token);
            }
            else
            {
                result = await _inventoryService.MoveItemAsync(
                    item,
                    newPosition,
                    itemView.LocalIsRotated,
                    _cancellationTokenSource.Token);
            }
            OnInventoryUpdated();

            if (!result.Success)
            {
                Debug.LogWarning($"Inventory operation failed: {result.ErrorMessage}");
                OnInventoryUpdated();
            }
        }

        private async void OnItemRemoved(InventoryItemView itemView)
        {
            var result = await _inventoryService.RemoveItemAsync(
                itemView.Item,
                _cancellationTokenSource.Token);

            if (!result.Success)
            {
                Debug.LogWarning($"Failed to remove item: {result.ErrorMessage}");
            }
        }
    }
}