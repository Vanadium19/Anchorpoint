using UnityEngine;
using Zenject;
using InventoryModule;

namespace EffectModule
{
    public class EffectInstaller : MonoInstaller
    {
        [SerializeField] private BuffCatalog buffCatalog;

        public override void InstallBindings()
        {
            if (buffCatalog != null)
            {
                Container.Bind<BuffCatalog>()
                    .FromInstance(buffCatalog)
                    .AsSingle();
            }

            Container.Bind<IPrimaryBuffTargetService>()
                .To<PrimaryBuffTargetService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<BuffService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IItemUseHandler>()
                .To<ItemUseHandler>()
                .AsSingle();

            Container.Bind<BuffSaveable>()
                .AsSingle();
        }
    }
}
