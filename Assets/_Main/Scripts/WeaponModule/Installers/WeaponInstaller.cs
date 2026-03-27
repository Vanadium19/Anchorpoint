using System.Collections.Generic;
using UnityEngine;
using Zenject;
using WeaponModule;

namespace WeaponModule.Installers
{
    public class WeaponInstaller : MonoInstaller
    {
        [SerializeField] private List<WeaponSetupData> loadout;

        public override void InstallBindings()
        {
            Container.BindInstance(loadout).AsSingle();

            Container.Bind<WeaponModel>().AsTransient();

            Container.BindFactory<WeaponConfig, WeaponView, WeaponController, WeaponFactory>();

            Container.Bind<WeaponInventory>()
                .AsSingle();

            Container.Bind<IWeaponInventory>()
                .To<WeaponInventory>()
                .FromResolve();
        }
    }
}