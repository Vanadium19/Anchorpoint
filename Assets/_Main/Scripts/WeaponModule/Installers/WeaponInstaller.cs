using UnityEngine;
using Zenject;
using System.Collections.Generic;
using WeaponModule.Configs;
using WeaponModule.Controllers;
using WeaponModule.Core;
using WeaponModule.View;

namespace WeaponModule.Installers
{
    public class WeaponInstaller : MonoInstaller
    {
        [Header("Inventory Setup")]
        [SerializeField] private List<WeaponSetupData> loadout;

        public override void InstallBindings()
        {
            Container.BindInstance(loadout).AsSingle();
            Container.Bind<WeaponModel>().AsTransient();
            Container.BindFactory<WeaponConfig, WeaponView, WeaponController, WeaponController.Factory>();
            Container.BindInterfacesAndSelfTo<WeaponInventory>().AsSingle();
        }
    }
}