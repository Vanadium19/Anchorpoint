using UnityEngine;
using Zenject;
using SaveModule;
using InventoryModule.ContextMenu;
using InventoryModule.ContextMenu.UI;

namespace InventoryModule
{
    public sealed class InventoryInstaller : MonoInstaller
    {
        [Header("Catalog")]
        [SerializeField] private ItemCatalog itemCatalog;

        [Header("UI References")]
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject externalPanel;
        [SerializeField] private CharacterInventory characterInventory;
        [SerializeField] private Canvas mainCanvas;

        [Header("Drop Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float dropDistance = 2f;
        [SerializeField] private float dropOffsetY = 0.5f;

        [Header("Context Menu")]
        [SerializeField] private ContainerWindow containerWindowPrefab;
        [SerializeField] private AbstractGrid gridPrefab;

        public override void InstallBindings()
        {
            Container.Bind<ItemCatalog>().FromInstance(itemCatalog).AsSingle();

            InstallGlobalBindings();
            InstallSceneBindings();
        }

        private void InstallGlobalBindings()
        {
            Container.BindInterfacesAndSelfTo<InventoryManager>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<EquipmentSlotService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<GridService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IContainerWindowService>()
                .To<ContainerWindowService>()
                .AsSingle();

            Container.Bind<IContextMenuStateService>()
                .To<ContextMenuStateService>()
                .AsSingle();

            Container.BindInterfacesTo<InventoryStaticDataResetHandler>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<UIInputHandler>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IContextActionService>()
                .To<ContextActionService>()
                .AsSingle();
        }

        private void InstallSceneBindings()
        {
            if (mainCanvas != null)
                Container.Bind<Canvas>().FromInstance(mainCanvas).AsSingle();

            Container.BindInterfacesTo<BuildingContainerService>()
                .AsSingle()
                .WithArguments(externalPanel);

            Container.Bind<IDropService>()
                .To<DropService>()
                .AsSingle()
                .WithArguments(playerTransform, dropDistance, dropOffsetY);

            if (containerWindowPrefab != null)
                Container.Bind<ContainerWindow>()
                    .FromInstance(containerWindowPrefab)
                    .AsSingle();

            if (gridPrefab != null)
                Container.Bind<AbstractGrid>()
                    .FromInstance(gridPrefab)
                    .AsSingle();

            if (characterInventory != null)
                Container.Bind<CharacterInventory>()
                    .FromInstance(characterInventory)
                    .AsSingle();

            if (inventoryUI != null)
            {
                Container.Bind<GameObject>()
                    .WithId("InventoryUI")
                    .FromInstance(inventoryUI);
            }

        Container.BindInterfacesTo<InventoryStartup>()
            .AsSingle();
    }

    public override void Start()
        {
            var contextActionService = Container.TryResolve<IContextActionService>() as ContextActionService;

            if (contextActionService != null)
                contextActionService.SetPrefabs(Container, containerWindowPrefab, gridPrefab, mainCanvas);
        }
    }
}
