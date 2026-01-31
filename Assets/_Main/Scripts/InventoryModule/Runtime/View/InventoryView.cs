using InputModule;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private RectTransform gridBackground;
        [SerializeField] private InventoryItemView itemPrefab;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private float tileSize = 64f;
        [SerializeField] private float spacing = 1f;

        public event Action<InventoryItemView, Vector2Int> ItemDropped;
        public event Action<InventoryItemView> ItemRemoved;
        public event Action<InventoryItemView> ItemDragStarted;
        public event Action<InventoryItemView> ItemDragEnded;

        private readonly List<InventoryItemView> _spawnedItems = new();
        private IItemProvider _itemProvider;
        private IInputMap _input;

        public bool IsVisible => windowRoot.activeSelf;

        public void Initialize(IItemProvider itemProvider, IInputMap input)
        {
            _itemProvider = itemProvider;
            _input = input;
            Hide();
        }

        public void GenerateGrid(int width, int height)
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

        public void Toggle(bool state)
        {
            windowRoot.SetActive(state);
            Cursor.visible = state;
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        }

        public void Hide() => Toggle(false);

        public void Render(IReadOnlyList<InventoryItem> items, IItemProvider itemProvider)
        {
            ClearItems();

            foreach (var item in items)
            {
                var itemDef = itemProvider.GetItemDefinition(item.Id);
                if (itemDef == null) continue;

                var itemView = Instantiate(itemPrefab, itemsContainer);
                itemView.Setup(item, itemDef, tileSize, spacing, () => _input.IsSplitPressed);
                itemView.DragStarted += OnItemDragStarted;
                itemView.DragEnded += OnItemDragEnded;
                itemView.DragUpdated += OnItemDragUpdated;

                _spawnedItems.Add(itemView);
            }
        }

        private void OnItemDragStarted(InventoryItemView itemView)
        {
            ItemDragStarted?.Invoke(itemView);
            itemView.transform.SetParent(windowRoot.transform, true);
        }

        private void OnItemDragUpdated(InventoryItemView itemView)
        {
            // Optional: visual feedback during drag
        }

        private void OnItemDragEnded(InventoryItemView itemView)
        {
            Vector2Int gridPos = GetGridPosition(itemView);
            itemView.transform.SetParent(itemsContainer, true);
            ItemDropped?.Invoke(itemView, gridPos);
            ItemDragEnded?.Invoke(itemView);
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

        private void ClearItems()
        {
            for (int i = _spawnedItems.Count - 1; i >= 0; i--)
            {
                if (_spawnedItems[i] != null)
                    Destroy(_spawnedItems[i].gameObject);
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