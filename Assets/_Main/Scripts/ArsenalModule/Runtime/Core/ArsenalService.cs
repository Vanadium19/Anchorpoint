using System;
using System.Collections.Generic;
using InventoryModule;
using UnityEngine;
using WeaponModule;
using Zenject;

namespace ArsenalModule
{
    /// <summary>
    /// Handles Arsenal UI screens and installs weapon modifications for a price.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>TradeModule.TradeService</c>: it is the <see cref="ExternalUIHandler"/> for every world
    /// object that exposes <see cref="IArsenalStation"/>. Modification rules live in
    /// <see cref="IWeaponModificationService"/>; this service only adds payment on top of them.
    /// </remarks>
    public class ArsenalService : ExternalUIHandler, IArsenalService
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IEquipmentSlotService _equipmentSlotService;
        private readonly IWeaponModificationService _weaponModificationService;
        private readonly Dictionary<IExternalUI, ArsenalPresenter> _presenters = new();

        /// <inheritdoc/>
        public event Action StateChanged;

        /// <summary>
        /// Creates the service with the inventory it charges, the weapon modification service it installs
        /// through, and the dependencies required to instantiate Arsenal UI screens.
        /// </summary>
        public ArsenalService(
            IInventoryManager inventoryManager,
            IEquipmentSlotService equipmentSlotService,
            IWeaponModificationService weaponModificationService,
            ExternalUIManager manager,
            DiContainer diContainer,
            Canvas canvas)
            : base(manager, diContainer, canvas)
        {
            _inventoryManager = inventoryManager;
            _equipmentSlotService = equipmentSlotService;
            _weaponModificationService = weaponModificationService;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
            _weaponModificationService.ModificationsChanged += OnModificationsChanged;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _weaponModificationService.ModificationsChanged -= OnModificationsChanged;
            base.Dispose();
        }

        /// <inheritdoc/>
        public IReadOnlyList<ItemTable> GetOwnedWeapons()
        {
            var weapons = new List<ItemTable>();
            var visited = new HashSet<ItemTable>();

            foreach (var slot in _equipmentSlotService.GetAllSlots())
            {
                if (slot != null && slot.IsEquipped)
                    CollectWeapons(slot.EquippedItem, weapons, visited);
            }

            CollectWeapons(_inventoryManager.MainGrid, weapons, visited);

            return weapons;
        }

        /// <inheritdoc/>
        public IReadOnlyList<ArsenalModificationPrice> GetOffers(ArsenalConfig config, ItemTable weapon)
        {
            var offers = new List<ArsenalModificationPrice>();

            if (config == null || weapon?.ItemDataSo is not WeaponItemSo weaponItem)
                return offers;

            foreach (var modification in weaponItem.AvailableModifications)
            {
                if (config.TryGetPrice(modification, out var price))
                    offers.Add(price);
            }

            return offers;
        }

        /// <inheritdoc/>
        public int GetOwnedCount(ItemDataSo item) => _inventoryManager.GetItemCount(item);

        /// <inheritdoc/>
        public bool CanAfford(ArsenalModificationPrice price)
        {
            if (price == null)
                return false;

            foreach (var cost in price.Cost)
            {
                if (_inventoryManager.GetItemCount(cost.Item) < cost.Count)
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool IsInstalled(ItemTable weapon, WeaponModificationConfig modification) =>
            _weaponModificationService.IsInstalled(weapon, modification);

        /// <inheritdoc/>
        public bool IsSlotOccupied(ItemTable weapon, WeaponModificationConfig modification)
        {
            if (weapon == null || modification == null || IsInstalled(weapon, modification))
                return false;

            foreach (var installed in _weaponModificationService.GetInstalled(weapon))
            {
                if (installed.Slot == modification.Slot)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool CanInstall(ItemTable weapon, ArsenalModificationPrice price) =>
            price != null
            && CanAfford(price)
            && _weaponModificationService.CanInstall(weapon, price.Modification);

        /// <inheritdoc/>
        public bool TryInstall(ItemTable weapon, ArsenalModificationPrice price)
        {
            if (!CanInstall(weapon, price))
                return false;

            foreach (var cost in price.Cost)
            {
                if (!_inventoryManager.TryRemoveItems(cost.Item, cost.Count))
                {
                    StateChanged?.Invoke();
                    return false;
                }
            }

            return _weaponModificationService.TryInstall(weapon, price.Modification);
        }

        /// <inheritdoc/>
        protected override bool CanHandle(IExternalUI ui) => ui is IArsenalStation;

        /// <inheritdoc/>
        protected override GameObject CreateView(IExternalUI ui)
        {
            var instance = InstantiateView(ui);
            var view = instance.GetComponent<ArsenalScreenView>();

            if (view != null && ui is IArsenalStation station)
            {
                var presenter = new ArsenalPresenter(this, view, station.Config);
                presenter.Initialize();
                _presenters[ui] = presenter;
            }

            return instance;
        }

        /// <inheritdoc/>
        protected override void OnReactivated(IExternalUI ui, GameObject view)
        {
            if (_presenters.TryGetValue(ui, out var presenter))
                presenter.Refresh();
        }

        private void OnModificationsChanged(ItemTable weapon) => StateChanged?.Invoke();

        private static void CollectWeapons(ItemTable item, List<ItemTable> weapons, HashSet<ItemTable> visited)
        {
            if (item == null || !visited.Add(item))
                return;

            if (item.ItemDataSo is WeaponItemSo)
                weapons.Add(item);

            if (!item.IsContainer)
                return;

            var metadata = item.GetMetadata<ContainerMetadata>();

            if (metadata == null)
                return;

            foreach (var grid in metadata.Inventories)
                CollectWeapons(grid, weapons, visited);
        }

        private static void CollectWeapons(GridTable grid, List<ItemTable> weapons, HashSet<ItemTable> visited)
        {
            if (grid == null)
                return;

            foreach (var item in grid.GetAllItems())
                CollectWeapons(item, weapons, visited);
        }
    }
}
