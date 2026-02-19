using UnityEngine;
using Zenject;
using InputModule;

namespace InventoryModule
{
    public sealed class InventorySceneInstaller : MonoInstaller
    {
        [Header("UI References")]
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private CharacterInventory characterInventory;
        [SerializeField] private Canvas mainCanvas;

        public override void InstallBindings()
        {
            if (mainCanvas != null)
            {
                Container.Bind<Canvas>().FromInstance(mainCanvas).AsSingle();
                Container.Bind<IItemDragGhostService>().To<ItemDragGhostService>().AsSingle();
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
