using UnityEngine;
using Zenject;
using EnemyModule.Configs;
using EnemyModule.Controllers;
using EnemyModule.Core;
using EnemyModule.View;

namespace EnemyModule.Installers
{
    public class EnemyInstaller : MonoInstaller
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private EnemyView view;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.BindInstance(view).AsSingle();

            Container.Bind<EnemyModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnemyController>().AsSingle();
        }
    }
}