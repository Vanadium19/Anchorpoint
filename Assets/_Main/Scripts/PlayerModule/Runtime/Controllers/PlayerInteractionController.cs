using Cysharp.Threading.Tasks;
using InputModule;
using InventoryModule;
using System;
using System.Threading;
using UIModule;
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
        private readonly PlayerConfig _config;
        private readonly UIModule.InteractionHUDView _hud;

        public event Action<LootItemView> HoverChanged;
        private LootItemView _currentHoveredLoot;

        public PlayerInteractionController(
            IInputMap input,
            IInventoryService inventoryService,
            IItemProvider itemProvider,
            Camera camera,
            PlayerConfig config)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _itemProvider = itemProvider ?? throw new ArgumentNullException(nameof(itemProvider));
            _camera = camera;
            _config = config;
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
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out var hit, _config.InteractionDistance, _config.InteractionLayer))
            {
                var loot = hit.collider.GetComponentInParent<LootItemView>();
                UpdateHover(loot);

                if (loot != null && _input.IsInteractPressed)
                {
                    TryPickUpLootAsync(loot).Forget();
                }
            }
            else
            {
                UpdateHover(null);
            }
        }
        private void UpdateHover(LootItemView newLoot)
        {
            if (_currentHoveredLoot == null && newLoot == null) return;
            if (_currentHoveredLoot != null && newLoot == null)
            {
                _currentHoveredLoot = null;
                HoverChanged?.Invoke(null);
                return;
            }

            if (_currentHoveredLoot == newLoot) return;

            _currentHoveredLoot = newLoot;
            HoverChanged?.Invoke(_currentHoveredLoot);
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
                    UpdateHover(null);
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