using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace WeaponModule.Installers
{
    public class WeaponInstaller : MonoInstaller
    {
        [SerializeField] private List<WeaponSetupData> loadout;
        [SerializeField] private AmmoView ammoView;

        public override void InstallBindings()
        {
            Container.BindInstance(loadout).AsSingle();

            if (ammoView != null)
                Container.BindInstance(ammoView).AsSingle();

            Container.Bind<AmmoReserveService>().AsSingle();

            Container.Bind<WeaponModel>().AsTransient();

            Container.BindFactory<WeaponConfig, WeaponView, WeaponController, WeaponFactory>();

            Container.BindInterfacesAndSelfTo<WeaponInventory>().AsSingle();

            if (ammoView != null)
                Container.BindInterfacesTo<AmmoPresenter>().AsSingle().NonLazy();
        }
    }
}