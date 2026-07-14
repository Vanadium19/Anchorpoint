using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponViewFactory : IWeaponViewFactory
    {
        private readonly DiContainer _container;
        private readonly WeaponContainer _weaponContainer;

        public WeaponViewFactory(DiContainer container, WeaponContainer weaponContainer)
        {
            _container = container;
            _weaponContainer = weaponContainer;
        }

        public WeaponView CreateView(WeaponItemSo weaponItem)
        {
            if (weaponItem == null || weaponItem.WeaponViewPrefab == null)
                return null;

            var view = _container.InstantiatePrefabForComponent<WeaponView>(
                weaponItem.WeaponViewPrefab, _weaponContainer.Container);

            return view;
        }
    }
}
