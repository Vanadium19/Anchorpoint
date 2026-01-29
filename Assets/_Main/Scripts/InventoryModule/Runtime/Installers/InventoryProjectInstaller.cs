using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class InventoryProjectInstaller : MonoInstaller
    {
        [SerializeField] private InventoryConfig inventoryConfig;
        [SerializeField] private ItemCatalog itemCatalog;

        public override void InstallBindings()
        {
            Container.BindInstance(inventoryConfig);
            Container.BindInstance(itemCatalog);
            Container.Bind<InventoryModel>()
                .AsSingle()
                .WithArguments(inventoryConfig.Width, inventoryConfig.Height);
        }
    }
}