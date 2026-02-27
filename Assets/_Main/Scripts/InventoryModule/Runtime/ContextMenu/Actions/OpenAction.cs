using UnityEngine;

namespace InventoryModule.ContextMenu.Actions
{
    public class OpenAction : ContextActionBase
    {
        private readonly ContainerWindow _containerWindowPrefab;
        private readonly AbstractGrid _gridPrefab;
        private readonly Canvas _canvas;

        public override string DisplayName => DisplayNameOverride ?? "Open";
        public override bool IsAvailable => Item.IsContainer;

        public OpenAction(
            ItemTable item, 
            string displayName,
            ContainerWindow containerWindowPrefab,
            AbstractGrid gridPrefab,
            Canvas canvas) 
            : base(item, displayName)
        {
            _containerWindowPrefab = containerWindowPrefab;
            _gridPrefab = gridPrefab;
            _canvas = canvas;
        }

        public override void Execute()
        {
            if (_containerWindowPrefab == null || _gridPrefab == null || _canvas == null)
            {
                return;
            }

            var metadata = Item.GetMetadata<ContainerMetadata>();
            if (metadata == null) return;

            ContainerWindow window = Object.Instantiate(_containerWindowPrefab, _canvas.transform);
            window.transform.SetAsLastSibling();
            window.Initialize(Item, metadata, _gridPrefab);
        }
    }
}
