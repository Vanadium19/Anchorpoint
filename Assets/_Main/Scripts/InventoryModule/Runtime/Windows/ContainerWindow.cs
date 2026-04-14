using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace InventoryModule
{
    public class ContainerWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform windowRect;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Settings")]
        [SerializeField] private AbstractGrid gridPrefab;
        [SerializeField] private float headerHeight = 50f;
        [SerializeField] private float padding = 20f;
        [SerializeField] private float minWindowWidth = 200f;
        [SerializeField] private float minWindowHeight = 150f;

        private readonly List<AbstractGrid> _contentGrids = new();

        private GameObject _panelInstance;
        private ItemTable _containerItem;
        private Canvas _canvas;
        private DiContainer _diContainer;

        public ItemTable ContainerItem => _containerItem;
        public AbstractGrid GridPrefab => gridPrefab;
        private IContainerWindowService _windowService;

        [Inject]
        public void Construct(DiContainer container)
        {
            _diContainer = container;
        }

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            _windowService?.UnregisterWindow(_containerItem, this);
        }

        public void Initialize(ItemTable containerItem, ContainerMetadata metadata, AbstractGrid gridPrefab, IContainerWindowService windowService = null)
        {
            _containerItem = containerItem;
            this.gridPrefab = gridPrefab;
            _windowService = windowService;

            _windowService?.RegisterWindow(containerItem, this);

            if (titleText != null)
                titleText.text = containerItem.ItemDataSo.DisplayName;

            if (contentContainer != null && metadata?.Inventories?.Count > 0)
                CreateContentGrids(containerItem, metadata, gridPrefab);

            _canvas = GetComponentInParent<Canvas>();
        }

        private void CreateContentGrids(ItemTable containerItem, ContainerMetadata metadata, AbstractGrid gridPrefab)
        {
            var containerGrids = containerItem.ItemDataSo.ContainerGrids;

            if (containerGrids != null)
            {
                var panelPrefab = containerGrids.ContainerPanelPrefab;

                if (panelPrefab != null)
                    CreateGridsFromPanel(panelPrefab, metadata, contentContainer);
                else
                    CreateGridsFromArray(containerGrids, metadata, gridPrefab, contentContainer);
            }
            else
                CreateSingleGrid(metadata.Inventories[0], gridPrefab, contentContainer);

            CalculateWindowSizeFromGrids();
        }

        private void CreateGridsFromPanel(GameObject panelPrefab, ContainerMetadata metadata, RectTransform parent)
        {
            _panelInstance = _diContainer != null
                ? _diContainer.InstantiatePrefab(panelPrefab, parent)
                : Instantiate(panelPrefab, parent);

            var panelGrids = _panelInstance.GetComponentsInChildren<AbstractGrid>();

            if (panelGrids != null && panelGrids.Length > 0 && metadata.Inventories.Count > 0)
            {
                for (int i = 0; i < panelGrids.Length && i < metadata.Inventories.Count; i++)
                {
                    var grid = panelGrids[i];
                    var gridTable = metadata.Inventories[i];

                    if (grid != null && gridTable != null)
                    {
                        grid.SetGridTableOnly(gridTable);
                        _contentGrids.Add(grid);
                    }
                }
            }
        }

        private void CreateGridsFromArray(ContainerGridsData containerGrids, ContainerMetadata metadata, AbstractGrid gridPrefab, RectTransform parent)
        {
            containerGrids.InitializeGrids();
            var prefabGrids = containerGrids.Grids;

            if (prefabGrids != null && prefabGrids.Length > 0 && metadata.Inventories.Count > 0)
            {
                for (int i = 0; i < prefabGrids.Length && i < metadata.Inventories.Count; i++)
                {
                    var prefabGrid = prefabGrids[i];
                    var gridTable = metadata.Inventories[i];

                    if (prefabGrid != null && gridTable != null)
                    {
                        AbstractGrid grid = _diContainer != null
                            ? _diContainer.InstantiatePrefabForComponent<AbstractGrid>(prefabGrid, parent)
                            : Instantiate(prefabGrid, parent);
                        grid.transform.localPosition = prefabGrid.transform.localPosition;
                        grid.RefreshGridFromTable(gridTable);
                        _contentGrids.Add(grid);
                    }
                }
            }
            else if (gridPrefab != null && metadata.Inventories.Count > 0)
                CreateSingleGrid(metadata.Inventories[0], gridPrefab, parent);
        }

        private void CreateSingleGrid(GridTable gridTable, AbstractGrid gridPrefab, RectTransform parent)
        {
            if (gridPrefab != null && gridTable != null)
            {
                AbstractGrid grid = _diContainer != null
                    ? _diContainer.InstantiatePrefabForComponent<AbstractGrid>(gridPrefab, parent)
                    : Instantiate(gridPrefab, parent);
                grid.OverrideGridSize(gridTable.Width, gridTable.Height);
                grid.RefreshGridFromTable(gridTable);
                _contentGrids.Add(grid);
            }
        }

        private void CalculateWindowSizeFromGrids()
        {
            if (windowRect == null || _contentGrids.Count == 0) 
                return;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float tileSize = 50f;

            foreach (var grid in _contentGrids)
            {
                if (grid == null) 
                    continue;

                var gridRect = grid.GetComponent<RectTransform>();

                if (gridRect == null) 
                    continue;

                tileSize = grid.TileSize;

                Vector3 pos = gridRect.localPosition;
                Vector2 size = gridRect.sizeDelta;

                minX = Mathf.Min(minX, pos.x);
                minY = Mathf.Min(minY, pos.y - size.y);
                maxX = Mathf.Max(maxX, pos.x + size.x);
                maxY = Mathf.Max(maxY, pos.y);
            }

            if (minX == float.MaxValue)
            {
                CalculateWindowSizeFallback();
                return;
            }

            float contentWidth = maxX - minX;
            float contentHeight = maxY - minY;

            float totalWidth = Mathf.Max(contentWidth + padding * 2, minWindowWidth);
            float totalHeight = Mathf.Max(contentHeight + headerHeight + padding * 2, minWindowHeight);

            windowRect.sizeDelta = new Vector2(totalWidth, totalHeight);

            if (contentContainer != null)
            {
                var contentRect = contentContainer.GetComponent<RectTransform>();

                if (contentRect != null)
                {
                    contentRect.anchorMin = Vector2.zero;
                    contentRect.anchorMax = Vector2.one;
                    contentRect.offsetMin = new Vector2(padding, padding);
                    contentRect.offsetMax = new Vector2(-padding, -headerHeight);
                }
            }

            if (_panelInstance != null)
            {
                var panelRect = _panelInstance.GetComponent<RectTransform>();

                if (panelRect != null)
                {
                    float prefabHeight = panelRect.sizeDelta.y;
                    panelRect.sizeDelta = new Vector2(totalWidth - padding * 2, prefabHeight);
                }
            }

            CenterContentIfNeeded(minX, maxX, minY, maxY);
        }

        private void CalculateWindowSizeFallback()
        {
            if (_contentGrids.Count > 0 && _contentGrids[0] != null)
            {
                var grid = _contentGrids[0];
                float tileSize = grid.TileSize;
                float gridWidthPx = grid.GridWidth * tileSize;
                float gridHeightPx = grid.GridHeight * tileSize;

                float totalWidth = Mathf.Max(gridWidthPx + padding * 2, minWindowWidth);
                float totalHeight = Mathf.Max(gridHeightPx + headerHeight + padding * 2, minWindowHeight);

                windowRect.sizeDelta = new Vector2(totalWidth, totalHeight);
            }
        }

        private void CenterContentIfNeeded(float minX, float maxX, float minY, float maxY)
        {
            if (contentContainer == null) 
                return;

            float contentWidth = maxX - minX;
            float contentHeight = maxY - minY;

            Vector2 windowSize = windowRect.sizeDelta;
            float availableWidth = windowSize.x - padding * 2;
            float availableHeight = windowSize.y - headerHeight - padding * 2;

            if (contentWidth < availableWidth || contentHeight < availableHeight)
            {
                float offsetX = (availableWidth - contentWidth) / 2f - minX;
                float offsetY = (availableHeight - contentHeight) / 2f - minY;

                foreach (var grid in _contentGrids)
                {
                    if (grid == null) 
                        continue;

                    var gridRect = grid.GetComponent<RectTransform>();

                    if (gridRect != null)
                        gridRect.localPosition += new Vector3(offsetX, offsetY, 0);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (windowRect != null)
                windowRect.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (windowRect != null && _canvas != null)
                windowRect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void Close()
        {
            _windowService?.UnregisterWindow(_containerItem, this);

            foreach (var grid in _contentGrids)
            {
                if (grid != null)
                    Destroy(grid.gameObject);
            }

            _contentGrids.Clear();
            _panelInstance = null;

            Destroy(gameObject);
        }
    }
}
