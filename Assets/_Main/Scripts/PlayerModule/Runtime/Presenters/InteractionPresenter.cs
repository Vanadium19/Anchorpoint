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
        private IContainerUI _lastContainer;

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
            _controller.ContainerHoverChanged += OnContainerHoverChanged;
        }

        public void Dispose()
        {
            _controller.LootHoverChanged -= OnLootHoverChanged;
            _controller.ContainerHoverChanged -= OnContainerHoverChanged;
        }

        private void OnLootHoverChanged(LootItemView loot)
        {
            if (loot == null)
            {
                _lastLoot = null;

                if (_lastContainer == null)
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

        private void OnContainerHoverChanged(IContainerUI container)
        {
            if (container == null)
            {
                _lastContainer = null;

                if (_lastLoot == null)
                    _view.Hide();

                return;
            }

            _lastContainer = container;
            var message = string.Format(_config.ContainerHintFormat,
                container.DisplayName);
            _view.Show(message);
        }
    }
}
