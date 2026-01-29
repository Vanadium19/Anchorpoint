using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule
{
    public class InventoryView : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private RectTransform gridBackground;

        [Header("Prefabs")]
        [SerializeField] private InventoryItemView itemPrefab;
        [SerializeField] private GameObject slotPrefab;

        [Header("Settings")]
        [SerializeField] private float tileSize = 64f;
        [SerializeField] private float spacing = 1f;

        private readonly List<InventoryItemView> _spawnedItems = new();
        private ItemCatalog _catalog;

        public bool IsVisible => windowRoot.activeSelf;

        public void Initialize(InventoryConfig config, ItemCatalog catalog)
        {
            _catalog = catalog;
            GenerateGrid(config.Width, config.Height);
            Hide();
        }

        public void Toggle(bool state)
        {
            windowRoot.SetActive(state);
            Cursor.visible = state;
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        }

        public void Hide() => Toggle(false);

        public void Render(IEnumerable<InventoryItem> items)
        {
            ClearItems();

            foreach (var item in items)
            {
                if (!_catalog.TryGetConfig(item.Id, out var config))
                    continue;

                var itemView = Instantiate(itemPrefab, itemsContainer);
                itemView.Setup(item, config, tileSize, spacing);
                _spawnedItems.Add(itemView);
            }
        }

        private void GenerateGrid(int width, int height)
        {
            foreach (Transform child in gridBackground)
                Destroy(child.gameObject);

            var layout = gridBackground.GetComponent<GridLayoutGroup>();
            if (layout != null)
            {
                layout.cellSize = new Vector2(tileSize, tileSize);
                layout.spacing = new Vector2(spacing, spacing);
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = width;
            }

            float totalWidth = (width * tileSize) + ((width - 1) * spacing);
            float totalHeight = (height * tileSize) + ((height - 1) * spacing);

            Vector2 size = new Vector2(totalWidth, totalHeight);
            gridBackground.sizeDelta = size;
            itemsContainer.sizeDelta = size;

            int totalSlots = width * height;
            for (int i = 0; i < totalSlots; i++)
            {
                Instantiate(slotPrefab, gridBackground);
            }
        }

        private void ClearItems()
        {
            foreach (var view in _spawnedItems)
            {
                if (view != null) Destroy(view.gameObject);
            }
            _spawnedItems.Clear();
        }
    }
}