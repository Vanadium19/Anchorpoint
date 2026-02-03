using InputModule;
using InventoryModule;
using System;
using UIModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public sealed class PlayerInteractionController : IInitializable, ITickable, IDisposable
    {
        private readonly IInputMap _input;
        private readonly IInventoryService _inventoryService;
        private readonly Camera _camera;
        private readonly PlayerConfig _config;
        private readonly UIModule.InteractionHUDView _hud;

        public event Action<LootItemView> HoverChanged;
        private LootItemView _currentHoveredLoot;

        public PlayerInteractionController(
            IInputMap input,
            IInventoryService inventoryService,
            PlayerConfig config)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _config = config;
            _camera = Camera.main;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
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
                    TryPickUpLoot(loot);
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

        private void TryPickUpLoot(LootItemView loot)
        {
            try
            {
                var itemDefinition = loot.ItemDef;

                if (itemDefinition == null)
                {
                    return;
                }

                var result = _inventoryService.AddItem(itemDefinition, loot.Amount);

                if (result.Success)
                {
                    loot.PlayCollectEffects();
                    UpdateHover(null);
                    UnityEngine.Object.Destroy(loot.gameObject);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
