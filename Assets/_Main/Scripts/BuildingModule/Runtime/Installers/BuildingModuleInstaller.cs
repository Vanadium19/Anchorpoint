using BuildingModule.Runtime.Commands;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingModuleInstaller : MonoInstaller
    {
        [SerializeField] private GridConfig gridConfig;
        [SerializeField] private BuildingCatalog buildingCatalog;

        [SerializeField] private LayerMask groundMask;

        [SerializeField] private GridView gridView;
        [SerializeField] private GameObject buildPanel;

        public override void InstallBindings()
        {
            var grid = CampGrid.CreateGrid(gridConfig);

            Container.Bind<GridConfig>().FromInstance(gridConfig).AsSingle();
            Container.Bind<BuildingCatalog>().FromInstance(buildingCatalog).AsSingle();

            Container.Bind<IGrid>().FromInstance(grid).AsSingle();

            Container.Bind<IPlacementService>().To<PlacementService>().AsSingle();
            Container.Bind<IPreviewService>().To<PreviewService>().AsSingle();
            Container.Bind<IStorageService>().To<StorageService>().AsSingle();
            Container.Bind<BuildingFactory>().AsSingle();

            Container.Bind<IConstructionModeService>().To<ConstructionModeService>().AsSingle();

            Container.BindInterfacesTo<PlacementServiceController>().AsSingle().WithArguments(groundMask).NonLazy();
            Container.BindInterfacesTo<ModeControllers>().AsSingle().NonLazy();

            Container.Bind<SelectBuildingCommand>().AsSingle().NonLazy();

            Container.BindInterfacesTo<ConstructionModePresenter>().AsSingle().WithArguments(buildPanel, gridView).NonLazy();
        }
    }
}