using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Zenject;

namespace InventoryModule
{
    public class ContainerSection : MonoBehaviour
    {
        private const int MaxRefreshAttempts = 5;
        private const float MinRefreshInterval = 0.016f;

        [Header("References")]
        [SerializeField] private RectTransform headerRect;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Image expandIcon;

        [Header("Settings")]
        [SerializeField] private Sprite expandedIcon;
        [SerializeField] private Sprite collapsedIcon;
        [SerializeField] private float collapsedHeight = 30f;

        private readonly List<AbstractGrid> _contentGrids = new();

        private ContainerMetadata _containerMetadata;
        private GridTable _cContainerGrid;
        private bool _isExpanded = true;

        private IInventoryManager _inventoryManager;

        private RectTransform _rectTransform;
        private LayoutElement _layoutElement;

        private bool _pendingLayoutUpdate;
        private float _lastRefreshTime;

        private bool _needsLateRefresh;
        private int _refreshAttempts;

        public ItemTable ContainerItem { get; private set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _layoutElement = GetComponent<LayoutElement>();

            if (_layoutElement == null)
                _layoutElement = gameObject.AddComponent<LayoutElement>();

            if (toggleButton != null)
                toggleButton.onClick.AddListener(OnToggleClicked);
        }

        private void LateUpdate()
        {
            if (!_needsLateRefresh || !gameObject.activeInHierarchy)
                return;

            _refreshAttempts++;

            var contentHeight = CalculateContentHeight();

            if (!(contentHeight > 0) && _refreshAttempts < MaxRefreshAttempts)
                return;

            RefreshVisuals();
            _needsLateRefresh = false;
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(OnToggleClicked);
        }

        [Inject]
        public void Construct(IInventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
        }

        public void InitializeContainer(ItemTable itemTable, ContainerMetadata metadata, AbstractGrid gridPrefab)
        {
            ContainerItem = itemTable;
            _containerMetadata = metadata;

            if (metadata?.Inventories?.Count > 0)
                _cContainerGrid = metadata.Inventories[0];

            if (_cContainerGrid != null)
                _inventoryManager?.RegisterAdditionalGrid(_cContainerGrid);

            if (titleText != null)
                titleText.text = itemTable.ItemDataSo.DisplayName;

            if (contentContainer != null)
            {
                var containerGrids = itemTable.ItemDataSo.ContainerGrids;

                if (containerGrids != null)
                {
                    var panelPrefab = containerGrids.ContainerPanelPrefab;

                    if (panelPrefab != null)
                    {
                        var panelInstance = Instantiate(panelPrefab, contentContainer);

                        var layoutElement = panelInstance.GetComponent<LayoutElement>();

                        if (layoutElement == null)
                            layoutElement = panelInstance.AddComponent<LayoutElement>();

                        layoutElement.ignoreLayout = true;

                        var panelGrids = containerGrids.GetGridsFromPanel(panelInstance);

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
                    else
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
                                    var grid = Instantiate(prefabGrid, contentContainer);
                                    grid.transform.localPosition = prefabGrid.transform.localPosition;
                                    grid.RefreshGridFromTable(gridTable);
                                    _contentGrids.Add(grid);
                                }
                            }
                        }
                        else if (gridPrefab != null && _cContainerGrid != null)
                        {
                            var grid = Instantiate(gridPrefab, contentContainer);
                            grid.OverrideGridSize(_cContainerGrid.Width, _cContainerGrid.Height);
                            grid.RefreshGridFromTable(_cContainerGrid);
                            _contentGrids.Add(grid);
                        }
                    }
                }
                else if (gridPrefab != null && _cContainerGrid != null)
                {
                    var grid = Instantiate(gridPrefab, contentContainer);
                    grid.OverrideGridSize(_cContainerGrid.Width, _cContainerGrid.Height);
                    grid.RefreshGridFromTable(_cContainerGrid);
                    _contentGrids.Add(grid);
                }
            }

            _isExpanded = true;

            RefreshVisuals();

            var contentHeight = CalculateContentHeight();

            if (!(contentHeight <= 0))
                return;

            _needsLateRefresh = true;
            _refreshAttempts = 0;
        }

        public void InitializeAsMainInventory(string sectionName, GridTable grid, AbstractGrid gridPrefab)
        {
            ContainerItem = null;
            _cContainerGrid = grid;

            if (titleText != null)
                titleText.text = sectionName;

            if (contentContainer != null && gridPrefab != null)
            {
                var newGrid = Instantiate(gridPrefab, contentContainer);
                newGrid.OverrideGridSize(grid.Width, grid.Height);
                newGrid.RefreshGridFromTable(grid);
                _contentGrids.Add(newGrid);
            }

            _isExpanded = true;

            RefreshVisuals();

            var contentHeight = CalculateContentHeight();

            if (contentHeight <= 0)
            {
                _needsLateRefresh = true;
                _refreshAttempts = 0;
            }
        }

        public void InitializeAsMainInventoryWithPanel(string sectionName, ContainerGridsData containerPanelPrefab, AbstractGrid fallbackGridPrefab, GridTable existingGrid)
        {
            ContainerItem = null;
            _cContainerGrid = existingGrid;

            if (titleText != null)
                titleText.text = sectionName;

            if (contentContainer != null && containerPanelPrefab != null)
            {
                var panelPrefab = containerPanelPrefab.ContainerPanelPrefab;

                if (panelPrefab != null)
                {
                    var panelInstance = Instantiate(panelPrefab, contentContainer);

                    var layoutElement = panelInstance.GetComponent<LayoutElement>();

                    if (layoutElement == null)
                        layoutElement = panelInstance.AddComponent<LayoutElement>();

                    layoutElement.ignoreLayout = true;

                    var panelGrids = panelInstance.GetComponentsInChildren<AbstractGrid>();

                    if (panelGrids != null && panelGrids.Length > 0)
                    {
                        foreach (var grid in panelGrids)
                        {
                            if (grid != null)
                            {
                                GridTable gridTable;

                                if (existingGrid != null)
                                    gridTable = existingGrid;
                                else
                                {
                                    gridTable = new GridTable(grid.GridWidth, grid.GridHeight);

                                    if (_cContainerGrid == null)
                                        _cContainerGrid = gridTable;
                                }

                                grid.SetGridTableOnly(gridTable);
                                _contentGrids.Add(grid);
                            }
                        }
                    }
                }
                else
                {
                    containerPanelPrefab.InitializeGrids();
                    var prefabGrids = containerPanelPrefab.Grids;

                    if (prefabGrids != null && prefabGrids.Length > 0)
                    {
                        foreach (var prefabGrid in prefabGrids)
                        {
                            if (prefabGrid != null)
                            {
                                AbstractGrid grid = Instantiate(prefabGrid, contentContainer);
                                grid.transform.localPosition = prefabGrid.transform.localPosition;

                                GridTable gridTable;

                                if (existingGrid != null)
                                    gridTable = existingGrid;
                                else
                                {
                                    gridTable = new(prefabGrid.GridWidth, prefabGrid.GridHeight);

                                    if (_cContainerGrid == null)
                                        _cContainerGrid = gridTable;
                                }

                                grid.RefreshGridFromTable(gridTable);
                                _contentGrids.Add(grid);
                            }
                        }
                    }
                    else if (fallbackGridPrefab != null)
                    {
                        AbstractGrid grid = Instantiate(fallbackGridPrefab, contentContainer);
                        GridTable gridTable = existingGrid ?? new GridTable(grid.GridWidth, grid.GridHeight);
                        _cContainerGrid = gridTable;
                        grid.RefreshGridFromTable(gridTable);
                        _contentGrids.Add(grid);
                    }
                }
            }
            else if (fallbackGridPrefab != null)
            {
                AbstractGrid grid = Instantiate(fallbackGridPrefab, contentContainer);
                GridTable gridTable = existingGrid ?? new GridTable(grid.GridWidth, grid.GridHeight);
                _cContainerGrid = gridTable;
                grid.RefreshGridFromTable(gridTable);
                _contentGrids.Add(grid);
            }

            _isExpanded = true;

            RefreshVisuals();

            var contentHeight = CalculateContentHeight();

            if (contentHeight <= 0)
            {
                _needsLateRefresh = true;
                _refreshAttempts = 0;
            }
        }

        public void RefreshGridUI()
        {
            if (_contentGrids == null || _containerMetadata == null)
                return;

            for (int i = 0; i < _contentGrids.Count && i < _containerMetadata.Inventories.Count; i++)
            {
                var grid = _contentGrids[i];
                var gridTable = _containerMetadata.Inventories[i];

                if (grid != null && gridTable != null)
                    grid.RefreshGridFromTable(gridTable);
            }

            RefreshVisuals();
        }

        public void RefreshGridUISafe()
        {
            if (!gameObject.activeInHierarchy)
            {
                RefreshGridUI();
                return;
            }

            var now = Time.unscaledTime;

            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                if (!_pendingLayoutUpdate)
                    DelayedRefreshAsync().Forget();

                return;
            }

            _lastRefreshTime = now;
            RefreshGridUI();
        }

        public void RefreshVisualsSafe()
        {
            if (!gameObject.activeInHierarchy)
            {
                RefreshVisuals();
                return;
            }

            var now = Time.unscaledTime;

            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                if (!_pendingLayoutUpdate)
                    DelayedVisualRefreshAsync().Forget();

                return;
            }

            _lastRefreshTime = now;
            RefreshVisuals();
        }

        public void Close()
        {
            if (_cContainerGrid != null)
                _inventoryManager?.UnregisterAdditionalGrid(_cContainerGrid);

            if (_contentGrids != null)
            {
                foreach (var grid in _contentGrids)
                {
                    if (grid != null)
                        Destroy(grid.gameObject);
                }

                _contentGrids.Clear();
            }

            Destroy(gameObject);
        }

        private float CalculateContentHeight()
        {
            if (_contentGrids == null || _contentGrids.Count == 0)
                return 0f;

            var maxHeight = 0f;

            foreach (var grid in _contentGrids)
            {
                if (grid == null)
                    continue;

                var gridRect = grid.GetRectTransform();

                if (gridRect == null)
                    continue;

                var gridHeight = Mathf.Abs(gridRect.anchoredPosition.y) + gridRect.sizeDelta.y;

                if (gridHeight > maxHeight)
                    maxHeight = gridHeight;
            }

            return maxHeight;
        }

        private void ForceUpdateParentLayout()
        {
            if (_pendingLayoutUpdate)
                return;

            var now = Time.unscaledTime;

            if (now - _lastRefreshTime < MinRefreshInterval)
                return;

            _pendingLayoutUpdate = true;
            _lastRefreshTime = now;

            LayoutUpdateAsync().Forget();
        }

        private void RefreshVisuals()
        {
            if (_rectTransform == null || _layoutElement == null)
                return;

            if (!_isExpanded)
            {
                _layoutElement.preferredHeight = collapsedHeight;
            }
            else
            {
                var contentHeight = CalculateContentHeight();
                _layoutElement.preferredHeight = collapsedHeight + contentHeight;
            }

            ForceUpdateParentLayout();
        }

        private void OnToggleClicked()
        {
            Toggle(!_isExpanded);
        }

        private void Toggle(bool expand)
        {
            _isExpanded = expand;

            if (contentContainer != null)
                contentContainer.gameObject.SetActive(_isExpanded);

            if (expandIcon != null)
                expandIcon.sprite = _isExpanded ? expandedIcon : collapsedIcon;

            if (gameObject.activeInHierarchy)
                RefreshAfterFrame().Forget();
            else
                RefreshVisuals();
        }

        private async UniTaskVoid DelayedVisualRefreshAsync()
        {
            await UniTask.Delay((int)(MinRefreshInterval * 1000), true);
            _lastRefreshTime = Time.unscaledTime;
            RefreshVisuals();
        }

        private async UniTaskVoid DelayedRefreshAsync()
        {
            await UniTask.Delay((int)(MinRefreshInterval * 1000), true);
            _lastRefreshTime = Time.unscaledTime;
            RefreshGridUI();
        }

        private async UniTaskVoid RefreshAfterFrame()
        {
            await UniTask.DelayFrame(1);
            RefreshVisuals();
        }

        private async UniTaskVoid LayoutUpdateAsync()
        {
            await UniTask.DelayFrame(1);

            var parent = transform.parent as RectTransform;

            if (parent != null)
            {
                var vlg = parent.GetComponent<VerticalLayoutGroup>();

                if (vlg != null)
                {
                    vlg.SetLayoutHorizontal();
                    vlg.SetLayoutVertical();
                }

                var csf = parent.GetComponent<ContentSizeFitter>();

                if (csf != null)
                {
                    csf.SetLayoutHorizontal();
                    csf.SetLayoutVertical();
                }
            }

            _pendingLayoutUpdate = false;
        }
    }
}