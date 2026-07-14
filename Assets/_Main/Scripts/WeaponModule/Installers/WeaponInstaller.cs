using UnityEngine;
using Zenject;

namespace WeaponModule.Installers
{
    public class WeaponInstaller : MonoInstaller
    {
        [SerializeField] private WeaponContainer weaponContainer;
        [SerializeField] private AmmoView ammoView;

        public override void InstallBindings()
        {
            if (weaponContainer != null)
                Container.BindInstance(weaponContainer).AsSingle();

            if (ammoView != null)
                Container.BindInstance(ammoView).AsSingle();

            Container.Bind<IWeaponViewFactory>().To<WeaponViewFactory>().AsSingle();
            Container.Bind<IWeaponStatsProvider>().To<WeaponStatsProvider>().AsSingle();
            Container.Bind<ICameraRecoilService>().To<CameraRecoilService>().AsSingle();
            Container.Bind<IBulletFactory>().To<BulletFactory>().AsSingle();

            Container.Bind<WeaponModel>().AsTransient();

            Container.BindInterfacesAndSelfTo<WeaponInventory>().AsSingle();

            if (ammoView != null)
                Container.BindInterfacesTo<AmmoPresenter>().AsSingle().NonLazy();
        }
    }
}
