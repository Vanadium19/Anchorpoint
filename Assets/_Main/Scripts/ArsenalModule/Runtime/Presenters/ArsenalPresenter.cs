using System.Collections.Generic;
using System.Linq;
using InventoryModule;
using UtilsModule;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// Wires an <see cref="ArsenalScreenView"/> to an <see cref="IArsenalService"/> for a single Arsenal.
    /// </summary>
    /// <remarks>
    /// Owns every dependency the view is not allowed to touch: it keeps the selected weapon, resolves text
    /// through <see cref="LocalizedText"/>, asks the service what can be installed and re-renders the
    /// view whenever <see cref="IArsenalService.StateChanged"/> fires.
    /// </remarks>
    public class ArsenalPresenter
    {
        private readonly IArsenalService _arsenalService;
        private readonly ArsenalScreenView _view;
        private readonly ArsenalConfig _config;
        private ItemTable _selectedWeapon;

        /// <summary>
        /// Creates the presenter for one Arsenal's view and price configuration.
        /// </summary>
        /// <param name="arsenalService">The service used to query and install modifications.</param>
        /// <param name="view">The view to render into.</param>
        /// <param name="config">The Arsenal's price configuration.</param>
        public ArsenalPresenter(IArsenalService arsenalService, ArsenalScreenView view, ArsenalConfig config)
        {
            _arsenalService = arsenalService;
            _view = view;
            _config = config;
        }

        /// <summary>
        /// Subscribes to view and service events and performs the first render.
        /// </summary>
        public void Initialize()
        {
            _view.WeaponSelected += OnWeaponSelected;
            _view.InstallRequested += OnInstallRequested;
            _view.Closed += Dispose;
            _arsenalService.StateChanged += OnStateChanged;

            if (_config != null && !string.IsNullOrEmpty(_config.TitleKey))
                _view.SetTitle(LocalizedText.Get(_config.TitleKey));

            Refresh();
        }

        /// <summary>
        /// Unsubscribes from the view and the service so the presenter no longer receives updates.
        /// </summary>
        public void Dispose()
        {
            _view.WeaponSelected -= OnWeaponSelected;
            _view.InstallRequested -= OnInstallRequested;
            _view.Closed -= Dispose;
            _arsenalService.StateChanged -= OnStateChanged;
        }

        /// <summary>
        /// Rebuilds display data for the owned weapons and the selected weapon's modifications and
        /// re-renders the view.
        /// </summary>
        public void Refresh()
        {
            var weapons = _arsenalService.GetOwnedWeapons();

            if (_selectedWeapon == null || !weapons.Contains(_selectedWeapon))
                _selectedWeapon = weapons.Count > 0 ? weapons[0] : null;

            _view.RenderWeapons(weapons.Select(BuildWeaponDisplayData).ToList());

            if (_selectedWeapon == null)
            {
                _view.RenderEmptyState(true);
                _view.RenderModifications(new List<ArsenalModificationDisplayData>());
                return;
            }

            _view.RenderEmptyState(false);
            _view.RenderSelectedWeapon(_selectedWeapon.ItemDataSo.DisplayName);
            _view.RenderModifications(
                _arsenalService.GetOffers(_config, _selectedWeapon)
                    .Select(BuildModificationDisplayData)
                    .ToList());
        }

        private ArsenalWeaponDisplayData BuildWeaponDisplayData(ItemTable weapon)
        {
            return new ArsenalWeaponDisplayData
            {
                Weapon = weapon,
                Title = weapon.ItemDataSo.DisplayName,
                Icon = weapon.ItemDataSo.Icon,
                IsSelected = weapon == _selectedWeapon
            };
        }

        private ArsenalModificationDisplayData BuildModificationDisplayData(ArsenalModificationPrice price)
        {
            var modification = price.Modification;

            return new ArsenalModificationDisplayData
            {
                Modification = modification,
                Title = Localize(modification.TitleKey),
                Description = Localize(modification.DescriptionKey),
                Icon = modification.Icon,
                Cost = price.Cost.Select(BuildCostDisplayData).ToList(),
                IsInstalled = _arsenalService.IsInstalled(_selectedWeapon, modification),
                IsSlotOccupied = _arsenalService.IsSlotOccupied(_selectedWeapon, modification),
                CanInstall = _arsenalService.CanInstall(_selectedWeapon, price)
            };
        }

        private ArsenalCostDisplayData BuildCostDisplayData(ArsenalCost cost)
        {
            return new ArsenalCostDisplayData
            {
                Icon = cost.Item != null ? cost.Item.Icon : null,
                Count = cost.Count,
                HasEnough = _arsenalService.GetOwnedCount(cost.Item) >= cost.Count
            };
        }

        private static string Localize(string key) => string.IsNullOrEmpty(key) ? string.Empty : LocalizedText.Get(key);

        private void OnWeaponSelected(ItemTable weapon)
        {
            _selectedWeapon = weapon;
            Refresh();
        }

        private void OnInstallRequested(WeaponModificationConfig modification)
        {
            if (_config != null && _config.TryGetPrice(modification, out var price))
                _arsenalService.TryInstall(_selectedWeapon, price);
        }

        private void OnStateChanged() => Refresh();
    }
}
