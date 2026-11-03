using System;
using System.Collections.Generic;
using InventoryModule;
using UnityEngine;
using WeaponModule;
using Zenject;

namespace ArsenalModule
{
    /// <summary>
    /// Handles Arsenal UI screens and installs weapon modifications against the player's inventory.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>TradeModule.TradeService</c>: it is the <see cref="ExternalUIHandler"/> for every
    /// <see cref="ArsenalView"/> and the single source of truth for whether a modification can be installed.
    /// </remarks>
    public class ArsenalService : ExternalUIHandler, IArsenalService
    {
        private static readonly EquipmentSlotType[] SlotTypes =
            (EquipmentSlotType[])Enum.GetValues(typeof(EquipmentSlotType));

        private readonly IInventoryManager _inventoryManager;
        private readonly IEquipmentSlotService _equipmentSlotService;
        private readonly IWeaponStatsProvider _statsProvider;
        private readonly IWeaponInventory _weaponInventory;
        private readonly Dictionary<IExternalUI, ArsenalPresenter> _presenters = new();

        /// <inheritdoc/>
        public event Action BuildChanged;

        /// <summary>
        /// Creates the service with the inventory and weapon dependencies it works against and the
        /// dependencies required to instantiate Arsenal UI screens.
        /// </summary>
        public ArsenalService(
            IInventoryManager inventoryManager,
            IEquipmentSlotService equipmentSlotService,
            IWeaponStatsProvider statsProvider,
            IWeaponInventory weaponInventory,
            ExternalUIManager manager,
            DiContainer diContainer,
            Canvas canvas)
            : base(manager, diContainer, canvas)
        {
            _inventoryManager = inventoryManager;
            _equipmentSlotService = equipmentSlotService;
            _statsProvider = statsProvider;
            _weaponInventory = weaponInventory;
        }

        /// <inheritdoc/>
        public IReadOnlyList<ItemTable> GetWeapons()
        {
            var weapons = new List<ItemTable>();
            var visited = new HashSet<ItemTable>();

            foreach (var slot in _equipmentSlotService.GetAllSlots())
                CollectItem(slot.EquippedItem, visited, weapons);

            foreach (var slotType in SlotTypes)
            {
                if (slotType != EquipmentSlotType.None)
                    CollectItem(_inventoryManager.GetEquippedItem(slotType), visited, weapons);
            }

            CollectGrid(_inventoryManager.MainGrid, visited, weapons);

            return weapons;
        }

        /// <inheritdoc/>
        public int GetOwnedCount(ItemDataSo item) => _inventoryManager.GetItemCount(item);

        /// <inheritdoc/>
        public IReadOnlyList<WeaponModificationConfig> GetAvailableModifications(ItemTable weapon)
        {
            if (weapon == null || weapon.ItemDataSo is not WeaponItemSo weaponItem)
                return Array.Empty<WeaponModificationConfig>();

            return _statsProvider.GetAvailableModifications(weaponItem);
        }

        /// <inheritdoc/>
        public WeaponBuild GetBuild(ItemTable weapon)
        {
            if (weapon == null || weapon.ItemDataSo is not WeaponItemSo weaponItem)
                return null;

            return _statsProvider.GetBuild(weaponItem, GetInstalledIds(weapon));
        }

        /// <inheritdoc/>
        public ArsenalInstallStatus GetInstallStatus(ItemTable weapon, WeaponModificationConfig modification)
        {
            if (weapon == null || weapon.ItemDataSo is not WeaponItemSo weaponItem)
                return ArsenalInstallStatus.NotCompatible;

            if (modification == null || string.IsNullOrEmpty(modification.Id) || !IsAvailable(weaponItem, modification))
                return ArsenalInstallStatus.NotCompatible;

            if (ContainsId(GetInstalledIds(weapon), modification.Id))
                return ArsenalInstallStatus.AlreadyInstalled;

            if (IsSlotOccupied(weapon, modification))
                return ArsenalInstallStatus.SlotOccupied;

            if (!HasResources(modification))
                return ArsenalInstallStatus.NotEnoughResources;

            return ArsenalInstallStatus.Available;
        }

        /// <inheritdoc/>
        public bool TryInstall(ItemTable weapon, WeaponModificationConfig modification)
        {
            if (GetInstallStatus(weapon, modification) != ArsenalInstallStatus.Available)
                return false;

            foreach (var cost in modification.Costs)
            {
                if (cost != null && cost.Item != null && cost.Count > 0)
                    _inventoryManager.TryRemoveItems(cost.Item, cost.Count);
            }

            weapon.InitializeWeaponModifications();
            weapon.WeaponModificationMetadata.AddModification(modification.Id);

            _weaponInventory.RefreshBuild(weapon);
            BuildChanged?.Invoke();

            return true;
        }

        /// <inheritdoc/>
        protected override bool CanHandle(IExternalUI ui) => ui is ArsenalView;

        /// <inheritdoc/>
        protected override GameObject CreateView(IExternalUI ui)
        {
            var instance = InstantiateView(ui);
            var view = instance.GetComponent<ArsenalScreenView>();

            if (view != null)
            {
                var presenter = new ArsenalPresenter(this, view, ui.DisplayName);
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

        private static IReadOnlyList<string> GetInstalledIds(ItemTable weapon)
        {
            var metadata = weapon.WeaponModificationMetadata;

            return metadata != null ? metadata.ModificationIds : Array.Empty<string>();
        }

        private static bool ContainsId(IReadOnlyList<string> ids, string id)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                    return true;
            }

            return false;
        }

        private static bool IsAvailable(WeaponItemSo weaponItem, WeaponModificationConfig modification) =>
            weaponItem.HasSocket(modification.SocketType);

        private static void CollectGrid(GridTable grid, HashSet<ItemTable> visited, List<ItemTable> weapons)
        {
            if (grid == null)
                return;

            foreach (var item in grid.GetAllItems())
                CollectItem(item, visited, weapons);
        }

        private static void CollectItem(ItemTable item, HashSet<ItemTable> visited, List<ItemTable> weapons)
        {
            if (item == null || !visited.Add(item))
                return;

            if (item.ItemDataSo is WeaponItemSo)
                weapons.Add(item);

            var container = item.GetMetadata<ContainerMetadata>();

            if (container == null)
                return;

            foreach (var grid in container.Inventories)
                CollectGrid(grid, visited, weapons);
        }

        private bool IsSlotOccupied(ItemTable weapon, WeaponModificationConfig modification)
        {
            var build = GetBuild(weapon);

            if (build == null)
                return false;

            foreach (var installed in build.Modifications)
            {
                if (installed.SocketType == modification.SocketType)
                    return true;
            }

            return false;
        }

        private bool HasResources(WeaponModificationConfig modification)
        {
            var required = new Dictionary<ItemDataSo, int>();

            foreach (var cost in modification.Costs)
            {
                if (cost == null || cost.Item == null || cost.Count <= 0)
                    continue;

                required.TryGetValue(cost.Item, out var total);
                required[cost.Item] = total + cost.Count;
            }

            foreach (var pair in required)
            {
                if (_inventoryManager.GetItemCount(pair.Key) < pair.Value)
                    return false;
            }

            return true;
        }
    }
}
