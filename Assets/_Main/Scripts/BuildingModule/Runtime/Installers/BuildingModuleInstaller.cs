using BaseModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingModuleInstaller : MonoInstaller
    {
        [SerializeField] private GridConfig gridConfig;
        [SerializeField] private BuildingCatalog buildingCatalog;
        [SerializeField] private PlacementConfig placementConfig;
        [SerializeField] private BuildingMenuConfig menuConfig;
        [SerializeField] private BaseLevelUIConfig baseLevelUIConfig;
        [SerializeField] private BaseLevelView baseLevelView;
        [SerializeField] private BuildingPricePanelView buildingPricePanelView;
        [SerializeField] private BuildingPriceUIConfig buildingPriceUIConfig;
        [SerializeField] private LayerMask raycastLayers;
        [SerializeField] private bool useGrid = true;

        [SerializeField] private GridView gridView;
        [SerializeField] private GameObject buildPanel;
        [SerializeField] private BuildingMenuView buildingMenuView;

        public override void InstallBindings()
        {
            var grid = CampGrid.CreateGrid(gridConfig);

            Container.Bind<GridConfig>().FromInstance(gridConfig).AsSingle();
            Container.Bind<BuildingCatalog>().FromInstance(buildingCatalog).AsSingle();
            Container.Bind<PlacementConfig>().FromInstance(placementConfig).AsSingle();
            Container.Bind<BuildingMenuConfig>().FromInstance(menuConfig).AsSingle();

            Container.Bind<IGrid>().FromInstance(grid).AsSingle();

            Container.Bind<IBuildingRegistry>()
                .To<BuildingRegistry>()
                .AsSingle();

            Container.Bind<IPlacementService>().To<PlacementService>().AsSingle();
            Container.Bind<IPreviewService>().To<PreviewService>().AsSingle();
            Container.Bind<IStorageService>().To<StorageService>().AsSingle();
            Container.Bind<BuildingFactory>().AsSingle();

            Container.Bind<IBuildingMenuService>().To<BuildingMenuService>().AsSingle();
            Container.Bind<IConstructionModeService>().To<ConstructionModeService>().AsSingle();

            Container.Bind<IPlacementInputHandler>()
            .To<PlacementInputHandler>()
            .AsSingle();

            Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();

            Container.BindInterfacesTo<PlacementController>()
            .AsSingle()
            .WithArguments(placementConfig, buildingCatalog, raycastLayers, useGrid)
            .NonLazy();
            Container.BindInterfacesTo<ModeControllers>().AsSingle().NonLazy();

            Container.BindInterfacesTo<ConstructionModePresenter>().AsSingle().WithArguments(buildPanel, gridView, useGrid).NonLazy();

            Container.Bind<BuildingMenuView>().FromInstance(buildingMenuView).AsSingle();
            Container.BindInterfacesTo<BuildingMenuPresenter>().AsSingle().WithArguments(buildingMenuView).NonLazy();

            Container.Bind<BaseLevelUIConfig>().FromInstance(baseLevelUIConfig).AsSingle();
            Container.Bind<BaseLevelView>().FromInstance(baseLevelView).AsSingle();
            Container.BindInterfacesTo<BaseLevelPreviewBridge>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BaseLevelPresenter>().AsSingle().NonLazy();

            Container.Bind<BuildingPriceUIConfig>().FromInstance(buildingPriceUIConfig).AsSingle();
            Container.Bind<BuildingPricePanelView>().FromInstance(buildingPricePanelView).AsSingle();
            Container.BindInterfacesTo<BuildingPricePresenter>().AsSingle().NonLazy();

            Container.BindInterfacesTo<WeaponBuildingModeHandler>().AsSingle().NonLazy();

            Container.Bind<IBuildingSaveService>()
                .To<BuildingSaveService>()
                .AsSingle();
        }
    }
}
