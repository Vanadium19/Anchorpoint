using UnityEngine;
using Zenject;
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
                Container.Bind<Canvas>().FromInstance(mainCanvas).AsSingle();

            Container.Bind<IDropService>()
                .To<DropService>()
                .AsSingle()
                .WithArguments(playerTransform, dropDistance, dropOffsetY);

            Container.BindInterfacesAndSelfTo<DeathLootSpawner>()
                .AsSingle()
                .WithArguments(1.25f, 0.35f)
                .NonLazy();

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
            contextActionService?.SetPrefabs(Container, containerWindowPrefab, gridPrefab, mainCanvas);
        }
    }
}