using InputModule;
using System;
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

        public event Action<InventoryItemView, Vector2Int> ItemDropped;

        private readonly List<InventoryItemView> _spawnedItems = new();
        private ItemCatalog _catalog;

        public bool IsVisible => windowRoot.activeSelf;
        private IInputMap _input;

        public void Initialize(InventoryConfig config, ItemCatalog catalog, IInputMap input)
        {
            _catalog = catalog;
            _input = input;
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
                if (!_catalog.TryGetConfig(item.Id, out var config)) continue;
                var itemView = Instantiate(itemPrefab, itemsContainer);
                itemView.Setup(item, config, tileSize, spacing, _input);
                itemView.DragStarted += OnItemDragStarted;
                itemView.DragEnded += OnItemDragEnded;
                itemView.DragUpdated += OnItemDragUpdated;
                _spawnedItems.Add(itemView);
            }
        }

        private void OnItemDragStarted(InventoryItemView itemView)
        {
            itemView.transform.SetParent(windowRoot.transform);
        }

        private void OnItemDragUpdated(InventoryItemView itemView)
        {
            Vector2Int gridPos = GetGridPosition(itemView);
            var rect = itemView.GetComponent<RectTransform>().rect;
            float cellSize = tileSize + spacing;
            int w = Mathf.RoundToInt(rect.width / cellSize);
            int h = Mathf.RoundToInt(rect.height / cellSize);
            if (w < 1) w = 1;
            if (h < 1) h = 1;
        }

        private void OnItemDragEnded(InventoryItemView itemView)
        {
            Vector2Int gridPos = GetGridPosition(itemView);
            ItemDropped?.Invoke(itemView, gridPos);
        }

        private Vector2Int GetGridPosition(InventoryItemView itemView)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemsContainer,
                Input.mousePosition,
                null,
                out var localMousePos
            );
            var rect = itemView.GetComponent<RectTransform>().rect;
            float itemTopLeftX = localMousePos.x - (rect.width * 0.5f);
            float itemTopLeftY = localMousePos.y + (rect.height * 0.5f);

            float cellSize = tileSize + spacing;

            int x = Mathf.RoundToInt(itemTopLeftX / cellSize);
            int y = Mathf.RoundToInt(-itemTopLeftY / cellSize);

            return new Vector2Int(x, y);
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

        public Vector2Int GetMouseGridPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemsContainer,
                Input.mousePosition,
                null,
                out Vector2 localPoint
            );

            float step = tileSize + spacing;
            int x = Mathf.FloorToInt(localPoint.x / step);
            int y = Mathf.FloorToInt(-localPoint.y / step);

            return new Vector2Int(x, y);
        }
    }
}