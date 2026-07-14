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
        private LootItemView _lastLoot;
        private IExternalUI _lastExternalUI;

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
            _controller.LootHoverChanged += OnLootHoverChanged;
            _controller.ExternalUIHoverChanged += OnExternalUIHoverChanged;
        }

        public void Dispose()
        {
            _controller.LootHoverChanged -= OnLootHoverChanged;
            _controller.ExternalUIHoverChanged -= OnExternalUIHoverChanged;
        }

        private void OnLootHoverChanged(LootItemView loot)
        {
            if (loot == null)
            {
                _lastLoot = null;

                if (_lastExternalUI == null)
                    _view.Hide();

                return;
            }

            if (_lastLoot == loot)
                return;

            _lastLoot = loot;
            var message = string.Format(_config.LootHintFormat,
                loot.ItemData?.DisplayName ?? "Item", loot.Amount);
            _view.Show(message);
        }

        private void OnExternalUIHoverChanged(IExternalUI externalUI)
        {
            if (externalUI == null)
            {
                _lastExternalUI = null;

                if (_lastLoot == null)
                    _view.Hide();

                return;
            }

            _lastExternalUI = externalUI;
            var message = string.Format(_config.ContainerHintFormat,
                externalUI.DisplayName);
            _view.Show(message);
        }
    }
}
