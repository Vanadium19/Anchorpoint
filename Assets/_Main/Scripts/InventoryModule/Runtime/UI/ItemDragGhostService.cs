using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace InventoryModule
{
    public class ItemDragGhostService : IItemDragGhostService
    {
        private readonly Canvas _canvas;
        private ItemDragGhost _ghost;

        public ItemDragGhostService(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Initialize()
        {
            if (_ghost != null) return;

            GameObject obj = new GameObject("ItemDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            obj.transform.SetParent(_canvas.transform, false);

            _ghost = obj.AddComponent<ItemDragGhost>();
            _ghost.Initialize(_canvas);
        }

        public void Show(ItemTable item, Vector2 position, Vector2 size)
        {
            if (_ghost == null) Initialize();
            _ghost.Show(item, position, size);
        }

        public void Hide()
        {
            _ghost?.Hide();
        }

        public void SetValid(bool isValid)
        {
            _ghost?.SetValid(isValid);
        }
    }
}
