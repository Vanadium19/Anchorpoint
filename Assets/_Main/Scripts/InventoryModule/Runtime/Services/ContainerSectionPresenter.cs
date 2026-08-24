using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InventoryModule
{
    public sealed class ContainerSectionPresenter
    {
        private const int MaxRefreshAttempts = 5;
        private const float MinRefreshInterval = 0.016f;

        private readonly IInventoryManager _inventoryManager;
        private readonly IContainerGridFactory _gridFactory;

        private readonly List<AbstractGrid> _contentGrids = new();

        private ContainerMetadata _containerMetadata;
        private GridTable _cContainerGrid;

        private bool _pendingLayoutUpdate;
        private float _lastRefreshTime;

        private bool _needsLateRefresh;
        private int _refreshAttempts;

        private Func<RectTransform> _layoutTargetGetter;

        public ItemTable ContainerItem { get; private set; }

        public ContainerSectionPresenter(IInventoryManager inventoryManager, IContainerGridFactory gridFactory)
        {
            _inventoryManager = inventoryManager;
            _gridFactory = gridFactory;
        }

        public IReadOnlyList<AbstractGrid> ContentGrids => _contentGrids;

        public void SetLayoutTargetGetter(Func<RectTransform> layoutTargetGetter)
        {
            _layoutTargetGetter = layoutTargetGetter;
        }

        public void InitializeContainer(ItemTable itemTable, ContainerMetadata metadata, AbstractGrid gridPrefab, Transform contentContainer)
        {
            ContainerItem = itemTable;
            _containerMetadata = metadata;

            var result = _gridFactory.BuildContainerGrids(itemTable, metadata, gridPrefab, contentContainer);

            _cContainerGrid = result.PrimaryGrid;
            _contentGrids.Clear();
            _contentGrids.AddRange(result.Grids);

            foreach (var grid in _contentGrids)
            {
                if (grid == null || grid.Grid == null)
                    continue;

                _inventoryManager?.RegisterAdditionalGrid(grid.Grid);
            }
        }

        public void InitializeAsMainInventory(GridTable grid, AbstractGrid gridPrefab, Transform contentContainer)
        {
            ContainerItem = null;
            _containerMetadata = null;
            _cContainerGrid = grid;

            var result = _gridFactory.BuildMainInventoryGrid(grid, gridPrefab, contentContainer);

            _cContainerGrid = result.PrimaryGrid;
            _contentGrids.Clear();
            _contentGrids.AddRange(result.Grids);

            RegisterSectionGrids();
        }

        public void InitializeAsMainInventoryWithPanel(ContainerGridsData containerPanelPrefab, AbstractGrid fallbackGridPrefab, GridTable existingGrid, Transform contentContainer)
        {
            ContainerItem = null;
            _containerMetadata = null;
            _cContainerGrid = existingGrid;

            var result = _gridFactory.BuildMainInventoryWithPanel(containerPanelPrefab, fallbackGridPrefab, existingGrid, contentContainer);

            _cContainerGrid = result.PrimaryGrid;
            _contentGrids.Clear();
            _contentGrids.AddRange(result.Grids);

            RegisterSectionGrids();
        }

        private void RegisterSectionGrids()
        {
            foreach (var grid in _contentGrids)
            {
                if (grid == null || grid.Grid == null || grid.Grid == _cContainerGrid)
                    continue;

                _inventoryManager?.RegisterAdditionalGrid(grid.Grid);
            }
        }

        public bool RefreshGridUI()
        {
            if (_contentGrids.Count == 0 || _containerMetadata == null)
                return false;

            for (int i = 0; i < _contentGrids.Count && i < _containerMetadata.Inventories.Count; i++)
            {
                var grid = _contentGrids[i];
                var gridTable = _containerMetadata.Inventories[i];

                if (grid != null && gridTable != null)
                    grid.RefreshGridFromTable(gridTable);
            }

            return true;
        }

        public void RefreshGridUISafe(Func<bool> isActiveInHierarchy, Action refreshVisuals)
        {
            bool refreshedNow;

            if (isActiveInHierarchy == null || !isActiveInHierarchy())
            {
                refreshedNow = RefreshGridUI();
                if (refreshedNow)
                    refreshVisuals?.Invoke();

                return;
            }

            var now = Time.unscaledTime;

            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                if (!_pendingLayoutUpdate)
                    DelayedRefreshAsync(refreshVisuals).Forget();

                return;
            }

            _lastRefreshTime = now;
            refreshedNow = RefreshGridUI();
            if (refreshedNow)
                refreshVisuals?.Invoke();
        }

        public void RefreshVisualsSafe(Func<bool> isActiveInHierarchy, Action refreshVisuals)
        {
            if (isActiveInHierarchy == null || !isActiveInHierarchy())
            {
                refreshVisuals?.Invoke();
                return;
            }

            var now = Time.unscaledTime;

            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                if (!_pendingLayoutUpdate)
                    DelayedVisualRefreshAsync(refreshVisuals).Forget();

                return;
            }

            _lastRefreshTime = now;
            refreshVisuals?.Invoke();
        }

        public void RequestLateRefresh()
        {
            _needsLateRefresh = true;
            _refreshAttempts = 0;
        }

        public void TickLateRefresh(Action refreshVisuals)
        {
            if (!_needsLateRefresh)
                return;

            _refreshAttempts++;

            var contentHeight = CalculateContentHeight();

            if (!(contentHeight > 0) && _refreshAttempts < MaxRefreshAttempts)
                return;

            refreshVisuals?.Invoke();
            _needsLateRefresh = false;
        }

        public float CalculateContentHeight()
        {
            if (_contentGrids.Count == 0)
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

        public void ForceUpdateParentLayout()
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

        public void Close()
        {
            if (_cContainerGrid != null)
                _inventoryManager?.UnregisterAdditionalGrid(_cContainerGrid);

            foreach (var grid in _contentGrids)
            {
                if (grid == null)
                    continue;

                if (grid.Grid != null && grid.Grid != _cContainerGrid)
                    _inventoryManager?.UnregisterAdditionalGrid(grid.Grid);

                UnityEngine.Object.Destroy(grid.gameObject);
            }

            _contentGrids.Clear();
        }

        private async UniTaskVoid DelayedVisualRefreshAsync(Action refreshVisuals)
        {
            await UniTask.Delay((int)(MinRefreshInterval * 1000), true);
            _lastRefreshTime = Time.unscaledTime;
            refreshVisuals?.Invoke();
        }

        private async UniTaskVoid DelayedRefreshAsync(Action refreshVisuals)
        {
            await UniTask.Delay((int)(MinRefreshInterval * 1000), true);
            _lastRefreshTime = Time.unscaledTime;
            if (RefreshGridUI())
                refreshVisuals?.Invoke();
        }

        private async UniTaskVoid LayoutUpdateAsync()
        {
            await UniTask.DelayFrame(1);

            var parent = _layoutTargetGetter?.Invoke();

            if (parent != null)
            {
                var vlg = parent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();

                if (vlg != null)
                {
                    vlg.SetLayoutHorizontal();
                    vlg.SetLayoutVertical();
                }

                var csf = parent.GetComponent<UnityEngine.UI.ContentSizeFitter>();

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
