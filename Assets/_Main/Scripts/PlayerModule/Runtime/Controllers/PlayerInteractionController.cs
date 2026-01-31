using Cysharp.Threading.Tasks;
using InputModule;
using InventoryModule;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public sealed class PlayerInteractionController : IInitializable, ITickable, IDisposable
    {
        private readonly IInputMap _input;
        private readonly IInventoryService _inventoryService;
        private readonly IItemProvider _itemProvider;
        private readonly Camera _camera;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public PlayerInteractionController(
            IInputMap input,
            IInventoryService inventoryService,
            IItemProvider itemProvider)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _itemProvider = itemProvider ?? throw new ArgumentNullException(nameof(itemProvider));
            _camera = Camera.main;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            // Initialization if needed
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public void Tick()
        {
            if (!_input.IsInteractPressed)
                return;

            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out var hit, 3f))
            {
                var loot = hit.collider.GetComponentInParent<LootItemView>();
                if (loot != null && loot.ItemDef != null)
                {
                    TryPickUpLootAsync(loot).Forget();
                }
            }
        }

        private async UniTaskVoid TryPickUpLootAsync(LootItemView loot)
        {
            var token = _cancellationTokenSource.Token;

            try
            {
                var itemDefinition = _itemProvider.GetItemDefinition(loot.ItemDef.Id);
                if (itemDefinition == null)
                {
                    Debug.LogWarning($"Item definition not found for loot: {loot.ItemDef.Id}");
                    return;
                }

                var result = await _inventoryService.AddItemAsync(
                    itemDefinition,
                    loot.Amount,
                    token);

                if (result.Success)
                {
                    loot.Collect();
                }
                else
                {
                    Debug.LogWarning($"Failed to pick up loot: {result.ErrorMessage}");
                }
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, ignore
            }
            catch (Exception exception)
            {
                Debug.LogError($"Error picking up loot: {exception}");
            }
        }
    }
}