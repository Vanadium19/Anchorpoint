using System;
using PlayerModule;
using WeaponModule;
using Zenject;

namespace BuildingModule
{
    public class WeaponBuildingModeHandler : IInitializable, IDisposable
    {
        private readonly IConstructionModeService _constructionModeService;
        private readonly PlayerProvider _playerProvider;

        public WeaponBuildingModeHandler(
            IConstructionModeService constructionModeService,
            PlayerProvider playerProvider)
        {
            _constructionModeService = constructionModeService;
            _playerProvider = playerProvider;
        }

        public void Initialize()
        {
            _constructionModeService.ActiveChanged += OnConstructionModeChanged;
        }

        public void Dispose()
        {
            _constructionModeService.ActiveChanged -= OnConstructionModeChanged;
        }

        private void OnConstructionModeChanged(bool isActive)
        {
            if (isActive && _playerProvider.TryGet<IWeaponInventory>(out var weaponInventory))
                weaponInventory.UnequipCurrentWeapon();
        }
    }
}