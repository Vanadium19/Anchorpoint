using UnityEngine;
using Zenject;
using System.Collections.Generic;

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

            Container.BindInterfacesTo<WeaponInventory>().AsSingle();
        }
    }
}