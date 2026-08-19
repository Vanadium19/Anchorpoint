using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public sealed class ContainerGridFactory : IContainerGridFactory
    {
        private readonly DiContainer _diContainer;

        public ContainerGridFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public ContainerGridBuildResult BuildContainerGrids(ItemTable itemTable, ContainerMetadata metadata, AbstractGrid fallbackGridPrefab, Transform contentContainer)
        {
            var result = new ContainerGridBuildResult();

            if (metadata?.Inventories?.Count > 0)
                result.PrimaryGrid = metadata.Inventories[0];

            if (contentContainer == null)
                return result;

            var containerGrids = itemTable?.ItemDataSo?.ContainerGrids;

            if (containerGrids == null)
            {
                if (fallbackGridPrefab != null && result.PrimaryGrid != null)
                {
                    var grid = InstantiateGrid(fallbackGridPrefab, contentContainer);
                    grid.OverrideGridSize(result.PrimaryGrid.Width, result.PrimaryGrid.Height);
                    grid.RefreshGridFromTable(result.PrimaryGrid);
                    result.Grids.Add(grid);
                }

                return result;
            }

            var panelPrefab = containerGrids.ContainerPanelPrefab;

            if (panelPrefab != null)
            {
                var panelInstance = InstantiatePanel(panelPrefab, contentContainer);

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
                            result.Grids.Add(grid);
                        }
                    }
                }

                return result;
            }

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
                        var grid = InstantiateGrid(prefabGrid, contentContainer);
                        grid.transform.localPosition = prefabGrid.transform.localPosition;
                        grid.RefreshGridFromTable(gridTable);
                        result.Grids.Add(grid);
                    }
                }
            }
            else if (fallbackGridPrefab != null && result.PrimaryGrid != null)
            {
                var grid = InstantiateGrid(fallbackGridPrefab, contentContainer);
                grid.OverrideGridSize(result.PrimaryGrid.Width, result.PrimaryGrid.Height);
                grid.RefreshGridFromTable(result.PrimaryGrid);
                result.Grids.Add(grid);
            }

            return result;
        }

        public ContainerGridBuildResult BuildMainInventoryGrid(GridTable grid, AbstractGrid gridPrefab, Transform contentContainer)
        {
            var result = new ContainerGridBuildResult
            {
                PrimaryGrid = grid
            };

            if (contentContainer == null || gridPrefab == null)
                return result;

            var newGrid = InstantiateGrid(gridPrefab, contentContainer);
            newGrid.OverrideGridSize(grid.Width, grid.Height);
            newGrid.RefreshGridFromTable(grid);
            result.Grids.Add(newGrid);

            return result;
        }

        public ContainerGridBuildResult BuildMainInventoryWithPanel(ContainerGridsData containerPanelPrefab, AbstractGrid fallbackGridPrefab, GridTable existingGrid, Transform contentContainer)
        {
            var result = new ContainerGridBuildResult
            {
                PrimaryGrid = existingGrid
            };

            if (contentContainer == null)
                return result;

            if (containerPanelPrefab != null)
            {
                var panelPrefab = containerPanelPrefab.ContainerPanelPrefab;

                if (panelPrefab != null)
                {
                    var panelInstance = InstantiatePanel(panelPrefab, contentContainer);

                    var panelGrids = panelInstance.GetComponentsInChildren<AbstractGrid>();

                    if (panelGrids != null && panelGrids.Length > 0)
                    {
                        foreach (var grid in panelGrids)
                        {
                            if (grid == null)
                                continue;

                            GridTable gridTable;

                            if (existingGrid != null)
                            {
                                gridTable = existingGrid;
                            }
                            else
                            {
                                gridTable = new GridTable(grid.GridWidth, grid.GridHeight);

                                if (result.PrimaryGrid == null)
                                    result.PrimaryGrid = gridTable;
                            }

                            grid.SetGridTableOnly(gridTable);
                            result.Grids.Add(grid);
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
                            if (prefabGrid == null)
                                continue;

                            var grid = InstantiateGrid(prefabGrid, contentContainer);
                            grid.transform.localPosition = prefabGrid.transform.localPosition;

                            GridTable gridTable;

                            if (existingGrid != null)
                            {
                                gridTable = existingGrid;
                            }
                            else
                            {
                                gridTable = new(prefabGrid.GridWidth, prefabGrid.GridHeight);

                                if (result.PrimaryGrid == null)
                                    result.PrimaryGrid = gridTable;
                            }

                            grid.RefreshGridFromTable(gridTable);
                            result.Grids.Add(grid);
                        }
                    }
                    else if (fallbackGridPrefab != null)
                    {
                        var grid = InstantiateGrid(fallbackGridPrefab, contentContainer);
                        var gridTable = existingGrid ?? new GridTable(grid.GridWidth, grid.GridHeight);
                        result.PrimaryGrid = gridTable;
                        grid.RefreshGridFromTable(gridTable);
                        result.Grids.Add(grid);
                    }
                }
            }
            else if (fallbackGridPrefab != null)
            {
                var grid = InstantiateGrid(fallbackGridPrefab, contentContainer);
                var gridTable = existingGrid ?? new GridTable(grid.GridWidth, grid.GridHeight);
                result.PrimaryGrid = gridTable;
                grid.RefreshGridFromTable(gridTable);
                result.Grids.Add(grid);
            }

            return result;
        }

        private AbstractGrid InstantiateGrid(AbstractGrid prefab, Transform parent)
        {
            return _diContainer != null
                ? _diContainer.InstantiatePrefabForComponent<AbstractGrid>(prefab, parent)
                : Object.Instantiate(prefab, parent);
        }

        private GameObject InstantiatePanel(GameObject prefab, Transform parent)
        {
            return _diContainer != null
                ? _diContainer.InstantiatePrefab(prefab, parent)
                : Object.Instantiate(prefab, parent);
        }
    }
}
