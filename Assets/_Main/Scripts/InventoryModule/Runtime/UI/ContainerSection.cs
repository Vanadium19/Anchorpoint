using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace InventoryModule
{
    public class ContainerSection : MonoBehaviour
    {
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

        public ItemTable ContainerItem { get; private set; }
        public ContainerMetadata ContainerMetadata { get; private set; }
        public GridTable ContainerGrid { get; private set; }
        public bool IsExpanded { get; private set; } = true;

        private List<AbstractGrid> _contentGrids = new List<AbstractGrid>();
        private RectTransform _rectTransform;
        private LayoutElement _layoutElement;

        private bool _pendingLayoutUpdate;
        private float _lastRefreshTime;
        private const float MinRefreshInterval = 0.016f;

        private bool _needsLateRefresh;
        private int _refreshAttempts;
        private const int MaxRefreshAttempts = 5;

        public void Initialize(ItemTable itemTable, GridTable grid)
        {
            ContainerItem = itemTable;
            ContainerGrid = grid;

            if (titleText != null)
            {
                titleText.text = itemTable.ItemDataSo.DisplayName;
            }

            RefreshVisuals();
        }

        public void InitializeContainer(ItemTable itemTable, ContainerMetadata metadata, AbstractGrid gridPrefab)
        {
            ContainerItem = itemTable;
            ContainerMetadata = metadata;

            if (metadata?.Inventories?.Count > 0)
            {
                ContainerGrid = metadata.Inventories[0];
            }

            if (ContainerGrid != null)
            {
                InventoryManager.Instance?.RegisterAdditionalGrid(ContainerGrid);
            }

            if (titleText != null)
            {
                titleText.text = itemTable.ItemDataSo.DisplayName;
            }

            if (contentContainer != null)
            {
                var containerGrids = itemTable.ItemDataSo.ContainerGrids;
                if (containerGrids != null)
                {
                    var panelPrefab = containerGrids.ContainerPanelPrefab;

                    if (panelPrefab != null)
                    {
                        GameObject panelInstance = Instantiate(panelPrefab, contentContainer);

                        var layoutElement = panelInstance.GetComponent<LayoutElement>();
                        if (layoutElement == null)
                        {
                            layoutElement = panelInstance.AddComponent<LayoutElement>();
                        }
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
                                    AbstractGrid grid = Instantiate(prefabGrid, contentContainer);
                                    grid.transform.localPosition = prefabGrid.transform.localPosition;
                                    grid.RefreshGridFromTable(gridTable);
                                    _contentGrids.Add(grid);
                                }
                            }
                        }
                        else if (gridPrefab != null && ContainerGrid != null)
                        {
                            AbstractGrid grid = Instantiate(gridPrefab, contentContainer);
                            grid.OverrideGridSize(ContainerGrid.Width, ContainerGrid.Height);
                            grid.RefreshGridFromTable(ContainerGrid);
                            _contentGrids.Add(grid);
                        }
                    }
                }
                else if (gridPrefab != null && ContainerGrid != null)
                {
                    AbstractGrid grid = Instantiate(gridPrefab, contentContainer);
                    grid.OverrideGridSize(ContainerGrid.Width, ContainerGrid.Height);
                    grid.RefreshGridFromTable(ContainerGrid);
                    _contentGrids.Add(grid);
                }
            }

            IsExpanded = true;

            RefreshVisuals();

            float contentHeight = CalculateContentHeight();
            if (contentHeight <= 0)
            {
                _needsLateRefresh = true;
                _refreshAttempts = 0;
            }
        }

        public void InitializeAsMainInventory(string sectionName, GridTable grid, AbstractGrid gridPrefab)
        {
            ContainerItem = null;
            ContainerGrid = grid;

            if (titleText != null)
            {
                titleText.text = sectionName;
            }

            if (contentContainer != null && gridPrefab != null)
            {
                AbstractGrid newGrid = Instantiate(gridPrefab, contentContainer);
                newGrid.OverrideGridSize(grid.Width, grid.Height);
                newGrid.RefreshGridFromTable(grid);
                _contentGrids.Add(newGrid);
            }

            IsExpanded = true;

            RefreshVisuals();

            float contentHeight = CalculateContentHeight();
            if (contentHeight <= 0)
            {
                _needsLateRefresh = true;
                _refreshAttempts = 0;
            }
        }

        public void InitializeAsMainInventoryWithPanel(string sectionName, ContainerGridsData containerPanelPrefab, AbstractGrid fallbackGridPrefab)
        {
            InitializeAsMainInventoryWithPanel(sectionName, containerPanelPrefab, fallbackGridPrefab, null);
        }

        public void InitializeAsMainInventoryWithPanel(string sectionName, ContainerGridsData containerPanelPrefab, AbstractGrid fallbackGridPrefab, GridTable existingGrid)
        {
            ContainerItem = null;
            ContainerGrid = existingGrid;

            if (titleText != null)
            {
                titleText.text = sectionName;
            }

            if (contentContainer != null && containerPanelPrefab != null)
            {
                var panelPrefab = containerPanelPrefab.ContainerPanelPrefab;

                if (panelPrefab != null)
                {
                    GameObject panelInstance = Instantiate(panelPrefab, contentContainer);

                    var layoutElement = panelInstance.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                    {
                        layoutElement = panelInstance.AddComponent<LayoutElement>();
                    }
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
                                {
                                    gridTable = existingGrid;
                                }
                                else
                                {
                                    gridTable = new GridTable(grid.GridWidth, grid.GridHeight);
                                    if (ContainerGrid == null)
                                    {
                                        ContainerGrid = gridTable;
                                    }
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
                                {
                                    gridTable = existingGrid;
                                }
                                else
                                {
                                    gridTable = new GridTable(prefabGrid.GridWidth, prefabGrid.GridHeight);
                                    if (ContainerGrid == null)
                                    {
                                        ContainerGrid = gridTable;
                                    }
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
                        ContainerGrid = gridTable;
                        grid.RefreshGridFromTable(gridTable);
                        _contentGrids.Add(grid);
                    }
                }
            }
            else if (fallbackGridPrefab != null)
            {
                AbstractGrid grid = Instantiate(fallbackGridPrefab, contentContainer);
                GridTable gridTable = existingGrid ?? new GridTable(grid.GridWidth, grid.GridHeight);
                ContainerGrid = gridTable;
                grid.RefreshGridFromTable(gridTable);
                _contentGrids.Add(grid);
            }

            IsExpanded = true;

            RefreshVisuals();

            float contentHeight = CalculateContentHeight();
            if (contentHeight <= 0)
            {
                _needsLateRefresh = true;
                _refreshAttempts = 0;
            }
        }

        private void OnContentGridReady()
        {
            RefreshVisuals();
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _layoutElement = GetComponent<LayoutElement>();

            if (_layoutElement == null)
            {
                _layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(OnToggleClicked);
            }
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(OnToggleClicked);
            }
        }

        private void LateUpdate()
        {
            if (_needsLateRefresh && gameObject.activeInHierarchy)
            {
                _refreshAttempts++;

                float contentHeight = CalculateContentHeight();
                if (contentHeight > 0 || _refreshAttempts >= MaxRefreshAttempts)
                {
                    RefreshVisuals();
                    _needsLateRefresh = false;
                }
            }
        }

        private void OnToggleClicked()
        {
            Toggle(!IsExpanded);
        }

        public void Toggle(bool expand)
        {
            IsExpanded = expand;

            if (contentContainer != null)
            {
                contentContainer.gameObject.SetActive(IsExpanded);
            }

            if (expandIcon != null)
            {
                expandIcon.sprite = IsExpanded ? expandedIcon : collapsedIcon;
            }

            if (gameObject.activeInHierarchy)
            {
                RefreshAfterFrame().Forget();
            }
            else
            {
                RefreshVisuals();
            }
        }

        private async UniTaskVoid RefreshAfterFrame()
        {
            await UniTask.DelayFrame(1);
            RefreshVisuals();
        }

        public void Expand() => Toggle(true);
        public void Collapse() => Toggle(false);

        private void RefreshVisuals()
        {
            if (_rectTransform == null || _layoutElement == null) return;

            if (!IsExpanded)
            {
                _layoutElement.preferredHeight = collapsedHeight;
            }
            else
            {
                float contentHeight = CalculateContentHeight();
                _layoutElement.preferredHeight = collapsedHeight + contentHeight;
            }

            ForceUpdateParentLayout();
        }

        private void ForceUpdateParentLayout()
        {
            if (_pendingLayoutUpdate) return;

            float now = Time.unscaledTime;
            if (now - _lastRefreshTime < MinRefreshInterval) return;

            _pendingLayoutUpdate = true;
            _lastRefreshTime = now;

            LayoutUpdateAsync().Forget();
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

        private float CalculateContentHeight()
        {
            if (_contentGrids == null || _contentGrids.Count == 0) return 0f;

            float maxHeight = 0f;
            foreach (var grid in _contentGrids)
            {
                if (grid != null)
                {
                    var gridRect = grid.GetRectTransform();
                    if (gridRect != null)
                    {
                        float gridHeight = Mathf.Abs(gridRect.anchoredPosition.y) + gridRect.sizeDelta.y;
                        if (gridHeight > maxHeight)
                            maxHeight = gridHeight;
                    }
                }
            }

            return maxHeight;
        }

        public void RefreshGridUI()
        {
            if (_contentGrids != null && ContainerMetadata != null)
            {
                for (int i = 0; i < _contentGrids.Count && i < ContainerMetadata.Inventories.Count; i++)
                {
                    var grid = _contentGrids[i];
                    var gridTable = ContainerMetadata.Inventories[i];
                    if (grid != null && gridTable != null)
                    {
                        grid.RefreshGridFromTable(gridTable);
                    }
                }
                RefreshVisuals();
            }
        }

        public void RefreshGridUISafe()
        {
            if (!gameObject.activeInHierarchy)
            {
                RefreshGridUI();
                return;
            }

            float now = Time.unscaledTime;
            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                if (!_pendingLayoutUpdate)
                {
                    DelayedRefreshAsync().Forget();
                }
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

            float now = Time.unscaledTime;
            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                if (!_pendingLayoutUpdate)
                {
                    DelayedVisualRefreshAsync().Forget();
                }
                return;
            }

            _lastRefreshTime = now;
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

        public void SetContentGrid(AbstractGrid grid)
        {
            _contentGrids.Clear();
            if (grid != null)
                _contentGrids.Add(grid);
        }

        public void SetContentGrids(List<AbstractGrid> grids)
        {
            _contentGrids = grids ?? new List<AbstractGrid>();
        }

        public AbstractGrid GetContentGrid()
        {
            return _contentGrids != null && _contentGrids.Count > 0 ? _contentGrids[0] : null;
        }

        public List<AbstractGrid> GetContentGrids()
        {
            return _contentGrids;
        }

        public void Close()
        {
            if (ContainerGrid != null)
            {
                InventoryManager.Instance?.UnregisterAdditionalGrid(ContainerGrid);
            }

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
    }
}
