using UnityEngine;
using Zenject;

namespace InventoryModule.ContextMenu.Actions
{
    public class OpenAction : ContextActionBase
    {
        private readonly ContainerWindow _containerWindowPrefab;
        private readonly AbstractGrid _gridPrefab;
        private readonly Canvas _canvas;
        private readonly IContainerWindowService _windowService;
        private readonly DiContainer _diContainer;

        public OpenAction(ItemTable item,
            string displayName,
            ContainerWindow containerWindowPrefab,
            AbstractGrid gridPrefab,
            Canvas canvas,
            IContainerWindowService windowService,
            DiContainer diContainer)
            : base(item, displayName)
        {
            _containerWindowPrefab = containerWindowPrefab;
            _gridPrefab = gridPrefab;
            _canvas = canvas;
            _windowService = windowService;
            _diContainer = diContainer;
        }

        public override string DisplayName => DisplayNameOverride ?? "Open";
        public override bool IsAvailable => Item.IsContainer && !_windowService.IsContainerOpen(Item);

        public override void Execute()
        {
            if (_containerWindowPrefab == null || _gridPrefab == null || _canvas == null)
                return;

            var metadata = Item.GetMetadata<ContainerMetadata>();

            if (metadata == null)
                return;

            ContainerWindow window = _diContainer != null
                ? _diContainer.InstantiatePrefabForComponent<ContainerWindow>(_containerWindowPrefab, _canvas.transform)
                : Object.Instantiate(_containerWindowPrefab, _canvas.transform);
            window.transform.SetAsLastSibling();
            window.Initialize(Item, metadata, _gridPrefab, _windowService);
        }
    }
}