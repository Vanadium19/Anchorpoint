using InputModule;
using System;
using System.Collections.Generic;
using TMPro;
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

        [Header("Tooltip Settings")]
        [SerializeField] private CanvasGroup tooltipCanvasGroup;
        [SerializeField] private TMP_Text tooltipText;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(15f, -15f);

        public event Action<InventoryItemView, Vector2Int, Vector2> ItemDropped;
        public event Action<InventoryItemView> ItemDragStarted;
        public event Action<InventoryItemView> ItemDragEnded;
        public event Action<InventoryItemView> ItemPointerEntered;
        public event Action<InventoryItemView> ItemPointerExited;

        private readonly List<InventoryItemView> _spawnedItems = new();
        private readonly Dictionary<InventoryItemView, Action<InventoryItemView>> _pointerEnteredHandlers = new();
        private readonly Dictionary<InventoryItemView, Action<InventoryItemView>> _pointerExitedHandlers = new();
        private IInventoryItemPool _itemPool;
        private IItemDatabase _itemDatabase;
        private IInputMap _input;
        private Vector2 _lastMousePosition;
        private Action<InventoryItemView, Vector2> _onItemDragEnded;

        public bool IsVisible => windowRoot.activeSelf;
        public InventoryItemView ItemPrefab => itemPrefab;
        public RectTransform ItemsContainer => itemsContainer;
        public void Hide() => Toggle(false);

        public void Initialize(IInventoryItemPool itemPool, IItemDatabase itemDatabase, IInputMap input)
        {
            _itemPool = itemPool;
            _itemDatabase = itemDatabase;
            _input = input;
            _onItemDragEnded = (item, pos) => OnItemDragEnded(item, pos);
            Hide();
        }

        private void Update()
        {
            _lastMousePosition = Input.mousePosition;
            if (tooltipCanvasGroup.alpha > 0)
            {
                UpdateTooltipPosition();
            }
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
            if (!state)
            {
                HideTooltip();
            }

            windowRoot.SetActive(state);

            Cursor.visible = state;
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        }

        public void Render(IReadOnlyList<InventoryItem> items)
        {
            ClearItems();

            foreach (var item in items)
            {
                var itemDef = _itemDatabase.GetItem(item.Id);
                if (itemDef == null)
                    continue;

                var itemView = _itemPool.Get();
                itemView.transform.SetParent(itemsContainer, false);
                itemView.Setup(item, itemDef, tileSize, spacing, () => _input.IsSplitPressed);
                
                itemView.DragStarted += OnItemDragStarted;
                itemView.DragEnded += _onItemDragEnded;
                itemView.DragUpdated += OnItemDragUpdated;
                
                Action<InventoryItemView> enteredHandler = view => ItemPointerEntered?.Invoke(view);
                Action<InventoryItemView> exitedHandler = view => ItemPointerExited?.Invoke(view);
                _pointerEnteredHandlers[itemView] = enteredHandler;
                _pointerExitedHandlers[itemView] = exitedHandler;
                
                itemView.PointerEntered += enteredHandler;
                itemView.PointerExited += exitedHandler;

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

        private void OnItemDragEnded(InventoryItemView itemView, Vector2 screenPosition)
        {
            Vector2Int gridPos = GetGridPosition(screenPosition, itemView.RectTransform);
            itemView.transform.SetParent(itemsContainer, true);
            ItemDropped?.Invoke(itemView, gridPos, screenPosition);
            ItemDragEnded?.Invoke(itemView);
        }

        private Vector2Int GetGridPosition(Vector2 screenPosition, RectTransform itemRectTransform)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemsContainer,
                screenPosition,
                null,
                out var localMousePos
            );
            var rect = itemRectTransform.rect;
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
                var itemView = _spawnedItems[i];
                if (itemView != null)
                {
                    itemView.DragStarted -= OnItemDragStarted;
                    itemView.DragEnded -= _onItemDragEnded;
                    itemView.DragUpdated -= OnItemDragUpdated;
                    
                    if (_pointerEnteredHandlers.TryGetValue(itemView, out var enteredHandler))
                    {
                        itemView.PointerEntered -= enteredHandler;
                        _pointerEnteredHandlers.Remove(itemView);
                    }
                    
                    if (_pointerExitedHandlers.TryGetValue(itemView, out var exitedHandler))
                    {
                        itemView.PointerExited -= exitedHandler;
                        _pointerExitedHandlers.Remove(itemView);
                    }
                    
                    _itemPool.Return(itemView);
                }
            }
            _spawnedItems.Clear();
        }

        public Vector2Int GetMouseGridPosition(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemsContainer,
                screenPosition,
                null,
                out var localPos
            );

            float cellSize = tileSize + spacing;
            int x = Mathf.FloorToInt(localPos.x / cellSize);
            int y = Mathf.FloorToInt(-localPos.y / cellSize);

            return new Vector2Int(x, y);
        }

        public bool IsMouseOverGrid(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemsContainer,
                screenPosition,
                null,
                out var localPos
            );

            return itemsContainer.rect.Contains(localPos);
        }

        public void ShowTooltip(string text)
        {
            if (tooltipText != null)
            {
                tooltipText.text = text;
                tooltipCanvasGroup.alpha = 1f;
            }
        }

        public void HideTooltip()
        {
            tooltipCanvasGroup.alpha = 0f;
        }

        private void UpdateTooltipPosition()
        {
            tooltipText.rectTransform.position = new Vector2(
                _lastMousePosition.x + tooltipOffset.x,
                _lastMousePosition.y + tooltipOffset.y);
        }
    }
}
