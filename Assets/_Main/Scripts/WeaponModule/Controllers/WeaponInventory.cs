using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponInventory : IWeaponInventory, IInitializable, IDisposable
    {
        private readonly IWeaponViewFactory _viewFactory;
        private readonly IWeaponStatsProvider _statsProvider;
        private readonly IEquipmentSlotService _slotService;
        private readonly DiContainer _container;

        private readonly Dictionary<ItemTable, WeaponController> _weaponMap = new();
        private readonly Dictionary<ItemTable, WeaponController> _weaponCache = new();
        private readonly List<EquipmentSlot> _subscribedSlots = new();

        private WeaponController _currentWeapon;

        public event Action<IWeapon> CurrentWeaponChanged;

        public WeaponInventory(
            IWeaponViewFactory viewFactory,
            IWeaponStatsProvider statsProvider,
            IEquipmentSlotService slotService,
            DiContainer container)
        {
            _viewFactory = viewFactory;
            _statsProvider = statsProvider;
            _slotService = slotService;
            _container = container;
        }

        public IWeapon CurrentWeapon => _currentWeapon;

        public int WeaponsCount => _weaponMap.Count;

        public void Initialize()
        {
            SubscribeToSlots();
            ScanEquippedWeapons();
        }

        public void Dispose()
        {
            foreach (var slot in _subscribedSlots)
            {
                slot.ItemEquipped -= OnItemEquipped;
                slot.ItemUnequipped -= OnItemUnequipped;
            }

            _subscribedSlots.Clear();

            DestroyAllWeapons();
        }

        public void EquipWeapon(ItemTable item)
        {
            if (item == null)
                return;

            if (!_weaponMap.TryGetValue(item, out var target))
                return;

            if (target == _currentWeapon)
            {
                UnequipCurrentWeapon();
                return;
            }

            if (_currentWeapon != null)
            {
                SaveAmmoToItem(_currentWeapon);
                _currentWeapon.Hide();
            }

            _currentWeapon = target;
            RestoreAmmoFromItem(_currentWeapon);
            _currentWeapon.Equip();
            CurrentWeaponChanged?.Invoke(_currentWeapon);
        }

        public void UnequipCurrentWeapon()
        {
            if (_currentWeapon == null)
                return;

            SaveAmmoToItem(_currentWeapon);
            _currentWeapon.Unequip().Forget();
            _currentWeapon = null;
            CurrentWeaponChanged?.Invoke(null);
        }

        private void SubscribeToSlots()
        {
            foreach (var slot in _slotService.GetAllSlots())
                SubscribeSlot(slot);
        }

        private void ScanEquippedWeapons()
        {
            foreach (var slot in _slotService.GetAllSlots())
            {
                if (slot.EquippedItem?.ItemDataSo is WeaponItemSo)
                    OnItemEquipped(slot.EquippedItem);
            }
        }

        private void SubscribeSlot(EquipmentSlot slot)
        {
            if (_subscribedSlots.Contains(slot))
                return;

            slot.ItemEquipped += OnItemEquipped;
            slot.ItemUnequipped += OnItemUnequipped;
            _subscribedSlots.Add(slot);
        }

        private void OnItemEquipped(ItemTable item)
        {
            if (item?.ItemDataSo is not WeaponItemSo weaponItem)
                return;

            if (_weaponMap.ContainsKey(item))
                return;

            if (_weaponCache.TryGetValue(item, out var cached))
            {
                _weaponCache.Remove(item);
                _weaponMap[item] = cached;

                if (_currentWeapon != null && _currentWeapon != cached)
                {
                    SaveAmmoToItem(_currentWeapon);
                    _currentWeapon.Hide();
                }

                _currentWeapon = cached;
                RestoreAmmoFromItem(cached);
                cached.Equip();
                CurrentWeaponChanged?.Invoke(cached);
                return;
            }

            var config = _statsProvider.GetConfig(weaponItem);

            if (config == null)
                return;

            var model = _container.Instantiate<WeaponModel>();
            var view = _viewFactory.CreateView(weaponItem);

            if (view == null)
                return;

            var controller = _container.Instantiate<WeaponController>(
                new object[] { config, model, view });

            controller.SetWeaponItem(weaponItem);
            controller.SetItemTable(item);

            int magAmmo = config.MagazineCapacity;
            int reserveAmmo = config.InitialReserveAmmo;

            if (item.WeaponAmmoMetadata != null)
            {
                magAmmo = item.WeaponAmmoMetadata.CurrentMagazineAmmo;
                reserveAmmo = item.WeaponAmmoMetadata.ReserveAmmo;
            }
            else
            {
                item.InitializeWeaponAmmo(config.MagazineCapacity, config.InitialReserveAmmo);
            }

            controller.InitializeFromItem(weaponItem, magAmmo, reserveAmmo);

            _weaponMap[item] = controller;

            if (_currentWeapon != null && _currentWeapon != controller)
            {
                SaveAmmoToItem(_currentWeapon);
                _currentWeapon.Hide();
            }

            _currentWeapon = controller;
            controller.Equip();
            CurrentWeaponChanged?.Invoke(controller);
        }

        private void OnItemUnequipped(ItemTable item)
        {
            if (!_weaponMap.TryGetValue(item, out var controller))
                return;

            SaveAmmoToItem(controller);

            if (_currentWeapon == controller)
            {
                _currentWeapon = null;
                CurrentWeaponChanged?.Invoke(null);
            }

            _weaponMap.Remove(item);
            controller.Hide();
            _weaponCache[item] = controller;
        }

        private void SaveAmmoToItem(WeaponController controller)
        {
            var itemTable = controller.GetItemTable();

            if (itemTable?.WeaponAmmoMetadata == null)
                return;

            controller.GetAmmoState(out int magAmmo, out int reserveAmmo);
            itemTable.WeaponAmmoMetadata.CurrentMagazineAmmo = magAmmo;
            itemTable.WeaponAmmoMetadata.ReserveAmmo = reserveAmmo;
        }

        private void RestoreAmmoFromItem(WeaponController controller)
        {
            var itemTable = controller.GetItemTable();

            if (itemTable?.WeaponAmmoMetadata == null)
                return;

            controller.SetAmmo(
                itemTable.WeaponAmmoMetadata.CurrentMagazineAmmo,
                itemTable.WeaponAmmoMetadata.ReserveAmmo);
        }

        private void DestroyAllWeapons()
        {
            foreach (var pair in _weaponMap)
            {
                pair.Value.Dispose();

                if (pair.Value.GetView() != null)
                    UnityEngine.Object.Destroy(pair.Value.GetView().gameObject);
            }

            foreach (var pair in _weaponCache)
            {
                pair.Value.Dispose();
                
                if (pair.Value.GetView() != null)
                    UnityEngine.Object.Destroy(pair.Value.GetView().gameObject);
            }

            _weaponMap.Clear();
            _weaponCache.Clear();
        }
    }
}
