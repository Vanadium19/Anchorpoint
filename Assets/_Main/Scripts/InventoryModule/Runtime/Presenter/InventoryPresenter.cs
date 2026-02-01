using Cysharp.Threading.Tasks;
using InputModule;
using System;
using System.Threading;
using UnityEngine;
using Zenject;
using ComponentsModule;

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
        private readonly IPlayerPositionProvider _playerPosition;
        private readonly InventoryConfig _config;
        private InventoryItemView _currentlyDraggedItem;
        private float _lastOpenTime;

        public InventoryPresenter(
            IInventoryService inventoryService,
            IItemProvider itemProvider,
            InventoryView view,
            IInputMap inputMap,
            IInputService inputService,
            IPlayerPositionProvider playerPosition,
            InventoryConfig config)
        {
            _inventoryService = inventoryService;
            _itemProvider = itemProvider;
            _view = view;
            _inputMap = inputMap;
            _inputService = inputService;
            _playerPosition = playerPosition;
            _config = config;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            _view.Initialize(_itemProvider, _inputMap);
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
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

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

        private async void OnItemDropped(InventoryItemView itemView, Vector2Int newPosition)
        {
            var item = itemView.Item;
            if (item == null) return;

            if (!_view.IsMouseOverGrid())
            {
                await HandleItemDiscard(itemView);
                return;
            }

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
        }

        private void OnItemPointerEntered(InventoryItemView itemView)
        {
            if (!_view.IsVisible || (Time.time - _lastOpenTime) < 0.1f)
                return;

            if (_itemProvider.TryGetItemDefinition(itemView.Item.Id, out var itemDef))
            {
                _view.ShowTooltip(itemDef.ItemName);
            }
        }

        private void OnItemPointerExited(InventoryItemView itemView)
        {
            _view.HideTooltip();
        }
        private async UniTask HandleItemDiscard(InventoryItemView itemView)
        {
            var item = itemView.Item;
            if (item == null) return;
            if (!_itemProvider.TryGetItemDefinition(item.Id, out var itemDef)) return;
            int amountToDiscard;
            if (itemView.IsSplitting)
            {
                amountToDiscard = item.Amount / 2;
                item.Amount -= amountToDiscard;
            }
            else
            {
                amountToDiscard = item.Amount;
                var result = await _inventoryService.RemoveItemAsync(item, _cancellationTokenSource.Token);

                if (!result.Success)
                {
                    return;
                }
            }
            SpawnItemInWorld(itemDef, amountToDiscard);
            OnInventoryUpdated();
        }

        private void SpawnItemInWorld(ItemDefinition itemDef, int amount)
        {
            if (itemDef.WorldPrefab == null)
            {
                return;
            }

            Vector3 spawnPos = _playerPosition.Position
                               + _playerPosition.Forward * _config.DiscardOffset
                               + Vector3.up * _config.DiscardUpOffset;

            LootItemView worldItem = GameObject.Instantiate(
                itemDef.WorldPrefab,
                spawnPos,
                _playerPosition.Rotation);

            worldItem.Initialize(itemDef, amount);

            if (worldItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(_playerPosition.Forward * _config.DiscardThrowForce, ForceMode.Impulse);
            }
        }
        private void OnInventoryUpdated()
        {
            if (_view == null) return;
            var items = _inventoryService.GetAllItems();
            _view.Render(items, _itemProvider);
        }
    }
}