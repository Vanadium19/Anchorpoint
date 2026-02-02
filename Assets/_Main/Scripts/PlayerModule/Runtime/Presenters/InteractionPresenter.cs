using System;
using Zenject;
using UIModule;
using InventoryModule;

namespace PlayerModule
{
    public class InteractionPresenter : IInitializable, IDisposable
    {
        private readonly PlayerInteractionController _controller;
        private readonly InteractionHUDView _view;
        private readonly PlayerConfig _config;
        private string _cachedMessage;
        private LootItemView _lastLoot;

        public InteractionPresenter(
            PlayerInteractionController controller,
            InteractionHUDView view,
            PlayerConfig config)
        {
            _controller = controller;
            _view = view;
            _config = config;
        }

        public void Initialize()
        {
            _view.Show("");
            _view.Hide();
            _controller.HoverChanged += OnHoverChanged;
        }

        public void Dispose()
        {
            _controller.HoverChanged -= OnHoverChanged;
        }

        private void OnHoverChanged(LootItemView loot)
        {
            if (loot == null)
            {
                _lastLoot = null;
                _view.Hide();
                return;
            }
            if (_lastLoot == loot) return;

            _lastLoot = loot;
            _cachedMessage = string.Format(_config.InteractionHintFormat, loot.ItemDef.ItemName, loot.Amount);
            _view.Show(_cachedMessage);
        }
    }
}