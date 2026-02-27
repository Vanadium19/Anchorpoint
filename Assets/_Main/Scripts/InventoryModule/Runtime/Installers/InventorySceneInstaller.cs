using UnityEngine;
using Zenject;
using InputModule;
using InventoryModule.ContextMenu;

namespace InventoryModule
{
    public sealed class InventorySceneInstaller : MonoInstaller
    {
        [Header("UI References")]
        [SerializeField] private GameObject inventoryUI;
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
            if (mainCanvas != null)
            {
                Container.Bind<Canvas>().FromInstance(mainCanvas).AsSingle();
            }

            Container.Bind<IItemDragGhostService>().To<ItemDragGhostService>().AsSingle();

            Container.Bind<IDropService>()
                .To<DropService>()
                .AsSingle()
                .WithArguments(playerTransform, dropDistance, dropOffsetY);

            if (containerWindowPrefab != null)
            {
                Container.Bind<ContainerWindow>()
                    .FromInstance(containerWindowPrefab)
                    .AsSingle();
            }

            if (gridPrefab != null)
            {
                Container.Bind<AbstractGrid>()
                    .FromInstance(gridPrefab)
                    .AsSingle();
            }

            var contextActionService = Container.TryResolve<ContextActionService>();
            if (contextActionService != null)
            {
                contextActionService.Initialize(
                    Container.TryResolve<IDropService>(),
                    containerWindowPrefab,
                    gridPrefab,
                    mainCanvas);
            }
        }

        public void Start()
        {
            var manager = Container.TryResolve<InventoryManager>();
            if (manager == null) return;

            var inputMap = Container.TryResolve<IInputMap>();
            var inputService = Container.TryResolve<IInputService>();

            manager.SetInput(inputMap, inputService);

            if (inventoryUI != null)
            {
                manager.SetInventoryUI(inventoryUI);
            }

            if (characterInventory != null)
            {
                characterInventory.Initialize(manager);
            }
        }
    }
}
