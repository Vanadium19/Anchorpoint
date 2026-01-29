using InventoryModule;
using InputModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public class PlayerInteractionController : ITickable
    {
        private readonly IInputMap _input;
        private readonly InventoryModel _inventory;
        private readonly Camera _camera;

        public PlayerInteractionController(IInputMap input, InventoryModel inventory)
        {
            _input = input;
            _inventory = inventory;
            _camera = Camera.main;
        }

        public void Tick()
        {
            if (!_input.IsInteractPressed) return;

            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out var hit, 3f))
            {
                var loot = hit.collider.GetComponentInParent<ILootable>();
                if (loot != null)
                {
                    if (_inventory.TryAddItem(loot.Config, loot.Amount))
                    {
                        loot.Collect();
                    }
                }
            }
        }
    }
}