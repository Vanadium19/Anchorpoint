using UnityEngine;
using Zenject;
using ComponentsModule;

namespace EnemyModule
{
    public class EnemyInstaller : MonoInstaller
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private EnemyView view;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(view).AsSingle();

            Container.Bind(typeof(IHealthComponent), typeof(IDamageable))
                .To<HealthComponent>()
                .AsSingle()
                .WithArguments(config.MaxHealth);

            Container.Bind<EnemyModel>().AsSingle();

            Container.BindInterfacesTo<EnemyController>()
                .AsSingle()
                .NonLazy();
        }
    }
}