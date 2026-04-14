using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace InventoryModule
{
    public abstract class AbstractGrid : MonoBehaviour
    {
        private const float MinRefreshInterval = 0.01f;

        [Header("Grid Configuration")]
        [SerializeField] protected int gridWidth = 10;
        [SerializeField] protected int gridHeight = 10;
        [SerializeField] protected float tileSize = 50f;

        [Header("Grid Colors")]
        [SerializeField] protected Color gridBackgroundColor = new(0.15f, 0.15f, 0.15f, 1f);
        [SerializeField] protected Color gridLineColor = new(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] [Range(1f, 4f)] protected float lineThickness = 2f;

        [Header("Highlight")]
        [SerializeField] protected Color highlightColor = new(0, 1, 0, 0.3f);
        [SerializeField] protected Color errorColor = new(1, 0, 0, 0.3f);

        private readonly List<AbstractItem> _itemUIs = new();

        private RectTransform _rectTransform;
        private Image _highlightImage;

        public GridTable Grid { get; private set; }
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float TileSize => tileSize;

        private bool _gridDrawn;
        private int _lastItemsHash;
        private float _lastRefreshTime;

        private IGridService _gridService;
        private DiContainer _diContainer;

        protected DiContainer DiContainer => _diContainer;

        [Inject]
        public void Construct(IGridService gridService, DiContainer container)
        {
            _gridService = gridService;
            _diContainer = container;
        }

        private void Awake()
        {
            if (_gridService != null)
                _gridService.RegisterGrid(this);

            _rectTransform = GetComponent<RectTransform>();

            _rectTransform.anchorMin = new(0, 1);
            _rectTransform.anchorMax = new(0, 1);
            _rectTransform.pivot = new(0, 1);
        }

        private void OnEnable()
        {
            if (Grid == null)
                return;

            Grid.ItemInserted += HandleItemInserted;
            Grid.ItemRemoved += HandleItemRemoved;
            RebuildGridUISmart();
        }

        private void Start()
        {
            if (Grid == null)
                InitializeGrid();
            else
                DrawGrid();

            CreateHighlightImage();
        }

        private void OnDisable()
        {
            if (Grid == null)
                return;

            Grid.ItemInserted -= HandleItemInserted;
            Grid.ItemRemoved -= HandleItemRemoved;
        }

        private void OnDestroy() => _gridService?.UnregisterGrid(this);

        public RectTransform GetRectTransform() => _rectTransform;

        public void UpdateItemPosition(AbstractItem itemUI)
        {
            if (itemUI?.Item?.Position == null)
                return;

            var itemRect = itemUI.GetComponent<RectTransform>();

            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);

            var itemW = itemUI.Item.Width;
            var itemH = itemUI.Item.Height;

            var x = itemUI.Item.Position.X * tileSize + (itemW * tileSize) / 2f;
            var y = -(itemUI.Item.Position.Y * tileSize) - (itemH * tileSize) / 2f;

            itemRect.localPosition = new Vector3(x, y, 0);
        }

        public Vector2Int GetGridPosition(Vector2 worldPosition)
        {
            var x = Mathf.FloorToInt(worldPosition.x / tileSize);
            var y = Mathf.FloorToInt(-worldPosition.y / tileSize);
            return new(x, y);
        }

        public void HideHighlight()
        {
            if (_highlightImage != null)
                _highlightImage.gameObject.SetActive(false);
        }

        public GridResponse TryPlaceItem(ItemTable item, int x, int y) => Grid.PlaceItem(item, x, y, item);

        public void RefreshGridFromTable(GridTable newTable)
        {
            var now = Time.unscaledTime;

            if (now - _lastRefreshTime < MinRefreshInterval)
                return;

            _lastRefreshTime = now;

            InternalRefreshGridFromTable(newTable, true);
        }

        public void ShowHighlight(int x, int y, int width, int height, bool isValid)
        {
            if (_highlightImage == null)
                return;

            _highlightImage.gameObject.SetActive(true);
            _highlightImage.color = isValid ? highlightColor : errorColor;
            _highlightImage.rectTransform.anchoredPosition = GetWorldPosition(x, y);
            _highlightImage.rectTransform.sizeDelta = new Vector2(width * tileSize, height * tileSize);
        }

        public void SetGridTableOnly(GridTable newTable)
        {
            if (newTable == null)
                return;

            if (Grid != null && isActiveAndEnabled)
            {
                Grid.ItemInserted -= HandleItemInserted;
                Grid.ItemRemoved -= HandleItemRemoved;
            }

            Grid = newTable;

            if (isActiveAndEnabled)
            {
                Grid.ItemInserted += HandleItemInserted;
                Grid.ItemRemoved += HandleItemRemoved;
            }

            OverrideGridSize(newTable.Width, newTable.Height);

            if (!_gridDrawn)
            {
                DrawGrid();
                _gridDrawn = true;
            }

            RebuildGridUISmart();
        }

        public void OverrideGridSize(int width, int height)
        {
            gridWidth = width;
            gridHeight = height;

            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = new Vector2(gridWidth * tileSize, gridHeight * tileSize);
            }
        }

        protected abstract AbstractItem InstantiateItemPrefab();

        private void InitializeGrid()
        {
            Grid = new(gridWidth, gridHeight);

            if (isActiveAndEnabled)
            {
                Grid.ItemInserted += HandleItemInserted;
                Grid.ItemRemoved += HandleItemRemoved;
            }

            _rectTransform.sizeDelta = new Vector2(gridWidth * tileSize, gridHeight * tileSize);
            _rectTransform.anchoredPosition = Vector2.zero;
            DrawGrid();
        }

        private void DrawGrid()
        {
            if (transform.Find("GridBackground") != null)
                return;

            var bgObj = new GameObject("GridBackground", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(transform, false);
            var bgImage = bgObj.GetComponent<Image>();
            bgImage.color = gridBackgroundColor;
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(gridWidth * tileSize, gridHeight * tileSize);

            for (int i = 0; i <= gridHeight; i++)
            {
                CreateGridLine(0, i * tileSize, gridWidth * tileSize, i * tileSize);
            }

            for (int i = 0; i <= gridWidth; i++)
            {
                CreateGridLine(i * tileSize, 0, i * tileSize, gridHeight * tileSize);
            }

            bgObj.transform.SetAsFirstSibling();
        }

        private void CreateGridLine(float x1, float y1, float x2, float y2)
        {
            var lineObj = new GameObject("GridLine", typeof(RectTransform), typeof(Image));
            lineObj.transform.SetParent(transform, false);
            var lineImage = lineObj.GetComponent<Image>();
            lineImage.color = gridLineColor;

            var lineRect = lineObj.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0, 1);
            lineRect.anchorMax = new Vector2(0, 1);
            lineRect.pivot = new Vector2(0, 1);

            var startPos = new Vector2(x1, -y1);
            var endPos = new Vector2(x2, -y2);

            var width = Mathf.Abs(endPos.x - startPos.x);
            var height = Mathf.Abs(endPos.y - startPos.y);

            var effectiveThickness = Mathf.Max(lineThickness, 2f);

            if (width < effectiveThickness)
                width = effectiveThickness;

            if (height < effectiveThickness)
                height = effectiveThickness;

            lineRect.anchoredPosition = new Vector2(Mathf.Min(startPos.x, endPos.x), Mathf.Max(startPos.y, endPos.y));
            lineRect.sizeDelta = new Vector2(width, height);

            lineObj.transform.SetAsFirstSibling();
        }

        private void CreateHighlightImage()
        {
            var highlightObj = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            highlightObj.transform.SetParent(transform, false);
            _highlightImage = highlightObj.GetComponent<Image>();
            _highlightImage.color = highlightColor;
            _highlightImage.raycastTarget = false;

            var highlightRect = highlightObj.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0, 1);
            highlightRect.anchorMax = new Vector2(0, 1);
            highlightRect.pivot = new Vector2(0, 1);
            highlightRect.anchoredPosition = Vector2.zero;

            highlightObj.SetActive(false);

            var borderObj = new GameObject("HighlightBorder", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(highlightObj.transform, false);
            var borderImage = borderObj.GetComponent<Image>();
            borderImage.color = new Color(0, 1, 0, 0.8f);
            borderImage.raycastTarget = false;

            var borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(0, 0);
            borderRect.offsetMax = new Vector2(0, 0);
        }

        private void HandleItemInserted(ItemTable item) => CreateItemUI(item);

        private void HandleItemRemoved(ItemTable item) => RemoveItemUI(item);

        private void CreateItemUI(ItemTable item)
        {
            var itemUI = InstantiateItemPrefab();
            itemUI.transform.SetParent(transform, false);

            var itemRect = itemUI.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);

            itemUI.SetItem(item);
            _itemUIs.Add(itemUI);
            UpdateItemPosition(itemUI);
        }

        private void RemoveItemUI(ItemTable item)
        {
            var itemUI = _itemUIs.Find(ui => ui.Item == item);

            if (itemUI == null)
                return;

            _itemUIs.Remove(itemUI);
            Destroy(itemUI.gameObject);
        }

        private Vector2 GetWorldPosition(int gridX, int gridY) => new(gridX * tileSize, -(gridY * tileSize));

        private void InternalRefreshGridFromTable(GridTable newTable, bool fullRebuild)
        {
            if (newTable == null || _rectTransform == null)
                return;

            if (Grid != null && isActiveAndEnabled)
            {
                Grid.ItemInserted -= HandleItemInserted;
                Grid.ItemRemoved -= HandleItemRemoved;
            }

            Grid = newTable;

            if (isActiveAndEnabled)
            {
                Grid.ItemInserted += HandleItemInserted;
                Grid.ItemRemoved += HandleItemRemoved;
            }

            OverrideGridSize(newTable.Width, newTable.Height);

            if (!_gridDrawn)
            {
                DrawGrid();
                _gridDrawn = true;
            }

            if (fullRebuild)
            {
                RebuildGridUISmart();
            }
        }

        private void RebuildGridUISmart()
        {
            var currentItems = Grid.GetAllItems();
            var currentHash = GetItemsHash(currentItems);

            if (currentHash == _lastItemsHash && _itemUIs.Count == currentItems.Length && _itemUIs.Count > 0)
            {
                foreach (var ui in _itemUIs)
                {
                    if (ui != null && ui.Item != null)
                        UpdateItemPosition(ui);
                }

                _lastItemsHash = currentHash;
                return;
            }

            _lastItemsHash = currentHash;

            var currentItemsSet = new HashSet<ItemTable>(currentItems);
            var itemsToRemove = new HashSet<AbstractItem>();

            foreach (var ui in _itemUIs)
            {
                if (ui == null || ui.Item == null || !currentItemsSet.Contains(ui.Item))
                    itemsToRemove.Add(ui);
            }

            foreach (var ui in itemsToRemove)
            {
                if (ui != null)
                {
                    _itemUIs.Remove(ui);
                    Destroy(ui.gameObject);
                }
            }

            foreach (var item in currentItems)
            {
                var found = false;

                foreach (var ui in _itemUIs)
                {
                    if (ui == null || ui.Item != item)
                        continue;

                    UpdateItemPosition(ui);
                    found = true;
                    break;
                }

                if (!found)
                    CreateItemUI(item);
            }
        }

        private int GetItemsHash(ItemTable[] items)
        {
            if (items == null || items.Length == 0)
                return 0;

            var hash = 17;

            foreach (var item in items)
            {
                if (item != null && item.Position != null)
                {
                    hash = hash * 31 + item.GetHashCode();
                    hash = hash * 31 + item.Position.X;
                    hash = hash * 31 + item.Position.Y;
                }
            }

            return hash;
        }
    }
}