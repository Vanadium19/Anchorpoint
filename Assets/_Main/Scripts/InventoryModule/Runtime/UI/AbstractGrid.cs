using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace InventoryModule
{
    public abstract class AbstractGrid : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] protected int gridWidth = 10;
        [SerializeField] protected int gridHeight = 10;
        [SerializeField] protected float tileSize = 50f;

        [Header("Grid Colors")]
        [SerializeField] protected Color gridBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        [SerializeField] protected Color gridLineColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] [Range(1f, 4f)] protected float lineThickness = 2f;

        [Header("Highlight")]
        [SerializeField] protected Color highlightColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] protected Color errorColor = new Color(1, 0, 0, 0.3f);

        public GridTable Grid { get; private set; }
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float TileSize => tileSize;

        public event Action GridReady;

        protected RectTransform rectTransform;
        protected Image highlightImage;
        protected List<AbstractItem> itemUIs = new List<AbstractItem>();

        private bool _gridDrawn;
        private int _lastItemsHash;
        private float _lastRefreshTime;
        private const float MinRefreshInterval = 0.01f;

        private IGridService _gridService;

        protected virtual void Awake()
        {
            _gridService = ProjectContext.Instance.Container.Resolve<IGridService>();
            _gridService?.RegisterGrid(this);

            rectTransform = GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
        }

        public void RestoreAnchors()
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
        }

        protected virtual void Start()
        {
            if (Grid == null)
            {
                InitializeGrid();
            }
            else
            {
                DrawGrid();
            }
            CreateHighlightImage();

            GridReady?.Invoke();
        }

        protected virtual void InitializeGrid()
        {
            Grid = new GridTable(gridWidth, gridHeight);

            if (isActiveAndEnabled)
            {
                Grid.ItemInserted += HandleItemInserted;
                Grid.ItemRemoved += HandleItemRemoved;
            }

            rectTransform.sizeDelta = new Vector2(gridWidth * tileSize, gridHeight * tileSize);
            rectTransform.anchoredPosition = Vector2.zero;

            DrawGrid();
        }

        protected virtual void DrawGrid()
        {
            if (transform.Find("GridBackground") != null)
            {
                return;
            }

            GameObject bgObj = new GameObject("GridBackground", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(transform, false);
            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.color = gridBackgroundColor;
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
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

        protected virtual void CreateGridLine(float x1, float y1, float x2, float y2)
        {
            GameObject lineObj = new GameObject("GridLine", typeof(RectTransform), typeof(Image));
            lineObj.transform.SetParent(transform, false);
            Image lineImage = lineObj.GetComponent<Image>();
            lineImage.color = gridLineColor;

            RectTransform lineRect = lineObj.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0, 1);
            lineRect.anchorMax = new Vector2(0, 1);
            lineRect.pivot = new Vector2(0, 1);

            Vector2 startPos = new Vector2(x1, -y1);
            Vector2 endPos = new Vector2(x2, -y2);

            float width = Mathf.Abs(endPos.x - startPos.x);
            float height = Mathf.Abs(endPos.y - startPos.y);

            if (width < lineThickness) width = lineThickness;
            if (height < lineThickness) height = lineThickness;

            lineRect.anchoredPosition = new Vector2(Mathf.Min(startPos.x, endPos.x), Mathf.Max(startPos.y, endPos.y));
            lineRect.sizeDelta = new Vector2(width, height);

            lineObj.transform.SetAsFirstSibling();
        }

        protected virtual void CreateCellBackground(int x, int y)
        {
            GameObject cellObj = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
            cellObj.transform.SetParent(transform, false);
            Image cellImage = cellObj.GetComponent<Image>();
            cellImage.color = new Color(1, 1, 1, 0.05f);
            cellImage.raycastTarget = true;

            RectTransform cellRect = cellObj.GetComponent<RectTransform>();
            cellRect.anchorMin = new Vector2(0, 1);
            cellRect.anchorMax = new Vector2(0, 1);
            cellRect.pivot = new Vector2(0, 1);
            cellRect.anchoredPosition = new Vector2(x * tileSize, -(y * tileSize));
            cellRect.sizeDelta = new Vector2(tileSize, tileSize);

            cellObj.transform.SetAsFirstSibling();
        }

        protected virtual void CreateHighlightImage()
        {
            GameObject highlightObj = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            highlightObj.transform.SetParent(transform, false);
            highlightImage = highlightObj.GetComponent<Image>();
            highlightImage.color = highlightColor;
            highlightImage.raycastTarget = false;

            RectTransform highlightRect = highlightObj.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0, 1);
            highlightRect.anchorMax = new Vector2(0, 1);
            highlightRect.pivot = new Vector2(0, 1);
            highlightRect.anchoredPosition = Vector2.zero;

            highlightObj.SetActive(false);

            GameObject borderObj = new GameObject("HighlightBorder", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(highlightObj.transform, false);
            Image borderImage = borderObj.GetComponent<Image>();
            borderImage.color = new Color(0, 1, 0, 0.8f);
            borderImage.raycastTarget = false;

            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(0, 0);
            borderRect.offsetMax = new Vector2(0, 0);
        }

        protected virtual void HandleItemInserted(ItemTable item)
        {
            CreateItemUI(item);
        }

        protected virtual void HandleItemRemoved(ItemTable item)
        {
            RemoveItemUI(item);
        }

        protected virtual void CreateItemUI(ItemTable item)
        {
            AbstractItem itemUI = InstantiateItemPrefab();
            itemUI.transform.SetParent(transform, false);

            RectTransform itemRect = itemUI.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);

            itemUI.SetItem(item);
            itemUIs.Add(itemUI);
            UpdateItemPosition(itemUI);
        }

        protected virtual void RemoveItemUI(ItemTable item)
        {
            AbstractItem itemUI = itemUIs.Find(ui => ui.Item == item);
            if (itemUI != null)
            {
                itemUIs.Remove(itemUI);
                Destroy(itemUI.gameObject);
            }
        }

        protected abstract AbstractItem InstantiateItemPrefab();

        public virtual void UpdateItemPosition(AbstractItem itemUI)
        {
            if (itemUI?.Item?.Position == null) return;

            RectTransform itemRect = itemUI.GetComponent<RectTransform>();

            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);

            int itemW = itemUI.Item.Width;
            int itemH = itemUI.Item.Height;

            float x = itemUI.Item.Position.X * tileSize + (itemW * tileSize) / 2f;
            float y = -(itemUI.Item.Position.Y * tileSize) - (itemH * tileSize) / 2f;

            itemRect.localPosition = new Vector3(x, y, 0);
        }

        public Vector2 GetWorldPosition(int gridX, int gridY)
        {
            return new Vector2(gridX * tileSize, -(gridY * tileSize));
        }

        public Vector2Int GetGridPosition(Vector2 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / tileSize);
            int y = Mathf.FloorToInt(-worldPosition.y / tileSize);
            return new Vector2Int(x, y);
        }

        public void ShowHighlight(int x, int y, int width, int height, bool isValid)
        {
            if (highlightImage == null) return;

            highlightImage.gameObject.SetActive(true);
            highlightImage.color = isValid ? highlightColor : errorColor;
            highlightImage.rectTransform.anchoredPosition = GetWorldPosition(x, y);
            highlightImage.rectTransform.sizeDelta = new Vector2(width * tileSize, height * tileSize);
        }

        public void HideHighlight()
        {
            if (highlightImage != null)
                highlightImage.gameObject.SetActive(false);
        }

        public bool IsPositionValid(int x, int y, int width, int height, ItemTable ignoreItem = null)
        {
            return Grid.OverlapCheck(x, y, width, height, ignoreItem) && Grid.BoundaryCheck(x, y, width, height);
        }

        public GridResponse TryPlaceItem(ItemTable item, int x, int y)
        {
            return Grid.PlaceItem(item, x, y, item);
        }

        public void OnDestroy()
        {
            _gridService?.UnregisterGrid(this);
        }

        protected virtual void OnEnable()
        {
            if (Grid != null)
            {
                Grid.ItemInserted += HandleItemInserted;
                Grid.ItemRemoved += HandleItemRemoved;
                RebuildGridUISmart();
            }
        }

        protected virtual void OnDisable()
        {
            if (Grid != null)
            {
                Grid.ItemInserted -= HandleItemInserted;
                Grid.ItemRemoved -= HandleItemRemoved;
            }
        }

        public void RefreshGridFromTable(GridTable newTable)
        {
            float now = Time.unscaledTime;
            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                return;
            }
            _lastRefreshTime = now;

            InternalRefreshGridFromTable(newTable, true);
        }

        public void RefreshGridFromTableImmediate(GridTable newTable)
        {
            InternalRefreshGridFromTable(newTable, true);
        }

        private void InternalRefreshGridFromTable(GridTable newTable, bool fullRebuild)
        {
            if (newTable == null || rectTransform == null) return;

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

        public void SetGridTableOnly(GridTable newTable)
        {
            if (newTable == null) return;

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

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(gridWidth * tileSize, gridHeight * tileSize);
            }
        }

        public RectTransform GetRectTransform()
        {
            return rectTransform;
        }

        private void RebuildGridUISmart()
        {
            var currentItems = Grid.GetAllItems();
            int currentHash = GetItemsHash(currentItems);

            if (currentHash == _lastItemsHash && itemUIs.Count > 0)
            {
                foreach (var ui in itemUIs)
                {
                    if (ui != null && ui.Item != null)
                    {
                        UpdateItemPosition(ui);
                    }
                }
                _lastItemsHash = currentHash;
                return;
            }

            _lastItemsHash = currentHash;

            HashSet<ItemTable> currentItemsSet = new HashSet<ItemTable>(currentItems);
            HashSet<AbstractItem> itemsToRemove = new HashSet<AbstractItem>();

            foreach (var ui in itemUIs)
            {
                if (ui == null || ui.Item == null || !currentItemsSet.Contains(ui.Item))
                {
                    itemsToRemove.Add(ui);
                }
            }

            foreach (var ui in itemsToRemove)
            {
                if (ui != null)
                {
                    itemUIs.Remove(ui);
                    Destroy(ui.gameObject);
                }
            }

            foreach (var item in currentItems)
            {
                bool found = false;
                foreach (var ui in itemUIs)
                {
                    if (ui != null && ui.Item == item)
                    {
                        UpdateItemPosition(ui);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    CreateItemUI(item);
                }
            }
        }

        private int GetItemsHash(ItemTable[] items)
        {
            if (items == null || items.Length == 0) return 0;

            int hash = 17;
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

        private void RebuildGridUI()
        {
            foreach (var ui in itemUIs)
            {
                if (ui != null) Destroy(ui.gameObject);
            }
            itemUIs.Clear();

            var items = Grid.GetAllItems();
            foreach (var item in items)
            {
                CreateItemUI(item);
            }
        }
    }
}
