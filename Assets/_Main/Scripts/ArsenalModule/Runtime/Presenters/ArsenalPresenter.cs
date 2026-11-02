using System;
using System.Collections.Generic;
using System.Globalization;
using InventoryModule;
using UtilsModule;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// Wires an <see cref="ArsenalScreenView"/> to an <see cref="IArsenalService"/> for one Arsenal.
    /// </summary>
    /// <remarks>
    /// Owns the weapon and modification selection, resolves every text through <see cref="LocalizedText"/>,
    /// previews a modification with <see cref="WeaponBuild.With"/> and re-renders the view whenever
    /// <see cref="IArsenalService.BuildChanged"/> fires or the screen is reopened.
    /// </remarks>
    public class ArsenalPresenter
    {
        private const string TitleKey = "arsenal.name";
        private const string WeaponsHeaderKey = "arsenal.header.weapons";
        private const string ModificationsHeaderKey = "arsenal.header.modifications";
        private const string StatsHeaderKey = "arsenal.header.stats";
        private const string InstallButtonKey = "arsenal.install";
        private const string EmptyWeaponsKey = "arsenal.empty.weapons";
        private const string EmptySelectionKey = "arsenal.empty.selection";
        private const string EmptyModificationsKey = "arsenal.empty.modifications";
        private const string DegreeSuffix = "°";

        private static readonly StatDefinition[] StatDefinitions =
        {
            new(WeaponStat.Damage, "arsenal.stat.damage", true, "0.##", string.Empty),
            new(WeaponStat.MagazineCapacity, "arsenal.stat.magazineCapacity", true, "0", string.Empty),
            new(WeaponStat.HipSpreadAngle, "arsenal.stat.hipSpread", false, "0.##", DegreeSuffix),
            new(WeaponStat.AimSpreadAngle, "arsenal.stat.aimSpread", false, "0.##", DegreeSuffix)
        };

        private readonly IArsenalService _service;
        private readonly ArsenalScreenView _view;
        private readonly string _title;

        private IReadOnlyList<ItemTable> _weapons = Array.Empty<ItemTable>();
        private ItemTable _selectedWeapon;
        private WeaponModificationConfig _selectedModification;

        /// <summary>
        /// Creates the presenter for one Arsenal screen.
        /// </summary>
        /// <param name="service">The service used to find weapons and install modifications.</param>
        /// <param name="view">The view to render into.</param>
        /// <param name="title">The localized title of the Arsenal building.</param>
        public ArsenalPresenter(IArsenalService service, ArsenalScreenView view, string title)
        {
            _service = service;
            _view = view;
            _title = title;
        }

        /// <summary>
        /// Subscribes to view and service events, sets the static texts and performs the first render.
        /// </summary>
        public void Initialize()
        {
            _view.WeaponSelected += OnWeaponSelected;
            _view.ModificationSelected += OnModificationSelected;
            _view.InstallRequested += OnInstallRequested;
            _view.Closed += Dispose;
            _service.BuildChanged += OnBuildChanged;

            _view.SetTexts(BuildTexts());

            Refresh();
        }

        /// <summary>
        /// Unsubscribes from the view and the service so the presenter no longer receives updates.
        /// </summary>
        public void Dispose()
        {
            _view.WeaponSelected -= OnWeaponSelected;
            _view.ModificationSelected -= OnModificationSelected;
            _view.InstallRequested -= OnInstallRequested;
            _view.Closed -= Dispose;
            _service.BuildChanged -= OnBuildChanged;
        }

        /// <summary>
        /// Reloads the weapon list, because the inventory may have changed, keeps the selection where
        /// possible and re-renders the view.
        /// </summary>
        public void Refresh()
        {
            _weapons = _service.GetWeapons();

            if (_selectedWeapon == null || !ContainsWeapon(_selectedWeapon))
                SelectWeapon(_weapons.Count > 0 ? _weapons[0] : null);

            EnsureModificationSelected();
            Render();
        }

        private static string Resolve(string key, string fallback) =>
            string.IsNullOrEmpty(key) ? fallback : LocalizedText.Get(key);

        private static string GetSlotText(WeaponModificationSlot slot) => slot switch
        {
            WeaponModificationSlot.Sight => LocalizedText.Get("arsenal.slot.sight"),
            WeaponModificationSlot.Magazine => LocalizedText.Get("arsenal.slot.magazine"),
            WeaponModificationSlot.Barrel => LocalizedText.Get("arsenal.slot.barrel"),
            _ => string.Empty
        };

        private static string GetStatusText(ArsenalInstallStatus status) => status switch
        {
            ArsenalInstallStatus.Available => LocalizedText.Get("arsenal.status.available"),
            ArsenalInstallStatus.AlreadyInstalled => LocalizedText.Get("arsenal.status.installed"),
            ArsenalInstallStatus.SlotOccupied => LocalizedText.Get("arsenal.status.slotOccupied"),
            ArsenalInstallStatus.NotEnoughResources => LocalizedText.Get("arsenal.status.notEnoughResources"),
            ArsenalInstallStatus.NotCompatible => LocalizedText.Get("arsenal.status.notCompatible"),
            _ => string.Empty
        };

        private static bool IsPreviewable(ArsenalInstallStatus status) =>
            status == ArsenalInstallStatus.Available || status == ArsenalInstallStatus.NotEnoughResources;

        private static string FormatValue(float value, string format, string suffix) =>
            value.ToString(format, CultureInfo.InvariantCulture) + suffix;

        private ArsenalScreenTexts BuildTexts()
        {
            return new ArsenalScreenTexts
            {
                TitleText = Resolve(TitleKey, _title),
                WeaponsHeaderText = LocalizedText.Get(WeaponsHeaderKey),
                ModificationsHeaderText = LocalizedText.Get(ModificationsHeaderKey),
                StatsHeaderText = LocalizedText.Get(StatsHeaderKey),
                InstallButtonText = LocalizedText.Get(InstallButtonKey)
            };
        }

        private bool ContainsWeapon(ItemTable weapon)
        {
            for (var i = 0; i < _weapons.Count; i++)
            {
                if (_weapons[i] == weapon)
                    return true;
            }

            return false;
        }

        private WeaponItemSo GetSelectedWeaponItem() =>
            _selectedWeapon != null ? _selectedWeapon.ItemDataSo as WeaponItemSo : null;

        private ArsenalInstallStatus GetSelectedStatus() =>
            _selectedModification != null
                ? _service.GetInstallStatus(_selectedWeapon, _selectedModification)
                : ArsenalInstallStatus.NotCompatible;

        private void SelectWeapon(ItemTable weapon)
        {
            _selectedWeapon = weapon;
            _selectedModification = null;
        }

        private void EnsureModificationSelected()
        {
            var weaponItem = GetSelectedWeaponItem();

            if (weaponItem == null)
            {
                _selectedModification = null;
                return;
            }

            if (_selectedModification != null && GetSelectedStatus() != ArsenalInstallStatus.NotCompatible)
                return;

            _selectedModification = FindDefaultModification(weaponItem);
        }

        private WeaponModificationConfig FindDefaultModification(WeaponItemSo weaponItem)
        {
            foreach (var modification in weaponItem.AvailableModifications)
            {
                if (modification != null && IsPreviewable(_service.GetInstallStatus(_selectedWeapon, modification)))
                    return modification;
            }

            return null;
        }

        private void Render()
        {
            var weaponItem = GetSelectedWeaponItem();

            _view.Render(new ArsenalScreenDisplayData
            {
                Weapons = BuildWeaponRows(),
                Modifications = BuildModificationRows(weaponItem),
                Stats = BuildStatLines(weaponItem),
                MessageText = ResolveMessage(weaponItem),
                HasSelectedWeapon = weaponItem != null,
                CanInstall = GetSelectedStatus() == ArsenalInstallStatus.Available
            });
        }

        private string ResolveMessage(WeaponItemSo weaponItem)
        {
            if (_weapons.Count == 0)
                return LocalizedText.Get(EmptyWeaponsKey);

            if (weaponItem == null)
                return LocalizedText.Get(EmptySelectionKey);

            if (weaponItem.AvailableModifications.Count == 0)
                return LocalizedText.Get(EmptyModificationsKey);

            return string.Empty;
        }

        private List<ArsenalWeaponDisplayData> BuildWeaponRows()
        {
            var rows = new List<ArsenalWeaponDisplayData>(_weapons.Count);

            foreach (var weapon in _weapons)
            {
                rows.Add(new ArsenalWeaponDisplayData
                {
                    Weapon = weapon,
                    NameText = weapon.ItemDataSo.DisplayName,
                    IsSelected = weapon == _selectedWeapon
                });
            }

            return rows;
        }

        private List<ArsenalModificationDisplayData> BuildModificationRows(WeaponItemSo weaponItem)
        {
            var rows = new List<ArsenalModificationDisplayData>();

            if (weaponItem == null)
                return rows;

            foreach (var modification in weaponItem.AvailableModifications)
            {
                if (modification != null)
                    rows.Add(BuildModificationRow(modification));
            }

            return rows;
        }

        private ArsenalModificationDisplayData BuildModificationRow(WeaponModificationConfig modification)
        {
            var status = _service.GetInstallStatus(_selectedWeapon, modification);

            return new ArsenalModificationDisplayData
            {
                Modification = modification,
                NameText = Resolve(modification.NameKey, modification.Id),
                DescriptionText = Resolve(modification.DescriptionKey, string.Empty),
                Icon = modification.Icon,
                SlotText = GetSlotText(modification.Slot),
                Status = status,
                StatusText = GetStatusText(status),
                IsInstalled = status == ArsenalInstallStatus.AlreadyInstalled,
                IsSelected = modification == _selectedModification,
                CanInstall = status == ArsenalInstallStatus.Available,
                Costs = BuildCostRows(modification)
            };
        }

        private List<ArsenalCostDisplayData> BuildCostRows(WeaponModificationConfig modification)
        {
            var rows = new List<ArsenalCostDisplayData>();

            foreach (var cost in modification.Costs)
            {
                if (cost == null || cost.Item == null)
                    continue;

                var owned = _service.GetOwnedCount(cost.Item);

                rows.Add(new ArsenalCostDisplayData
                {
                    ItemName = cost.Item.DisplayName,
                    Icon = cost.Item.Icon,
                    Owned = owned,
                    Required = cost.Count,
                    IsEnough = owned >= cost.Count
                });
            }

            return rows;
        }

        private List<ArsenalStatDisplayData> BuildStatLines(WeaponItemSo weaponItem)
        {
            var lines = new List<ArsenalStatDisplayData>();
            var current = _service.GetBuild(_selectedWeapon);

            if (weaponItem == null || current == null)
                return lines;

            var preview = BuildPreview(weaponItem, current);

            foreach (var definition in StatDefinitions)
                lines.Add(BuildStatLine(definition, current, preview));

            return lines;
        }

        private WeaponBuild BuildPreview(WeaponItemSo weaponItem, WeaponBuild current)
        {
            if (_selectedModification == null || !IsPreviewable(GetSelectedStatus()))
                return null;

            return current.With(weaponItem.WeaponConfig, _selectedModification);
        }

        private ArsenalStatDisplayData BuildStatLine(
            StatDefinition definition,
            WeaponBuild current,
            WeaponBuild preview)
        {
            var currentValue = current.Stats.Get(definition.Stat);
            var currentText = FormatValue(currentValue, definition.Format, definition.Suffix);
            var line = new ArsenalStatDisplayData
            {
                LabelText = LocalizedText.Get(definition.LabelKey),
                CurrentValueText = currentText,
                PreviewValueText = currentText
            };

            if (preview == null)
                return line;

            var previewValue = preview.Stats.Get(definition.Stat);
            var previewText = FormatValue(previewValue, definition.Format, definition.Suffix);

            line.PreviewValueText = previewText;
            line.HasPreview = previewText != currentText;
            line.IsImprovement = definition.IsHigherBetter ? previewValue > currentValue : previewValue < currentValue;

            return line;
        }

        private void OnWeaponSelected(ItemTable weapon)
        {
            if (weapon == _selectedWeapon)
                return;

            SelectWeapon(weapon);
            EnsureModificationSelected();
            Render();
        }

        private void OnModificationSelected(WeaponModificationConfig modification)
        {
            _selectedModification = modification;
            Render();
        }

        private void OnInstallRequested()
        {
            if (_selectedWeapon == null || _selectedModification == null)
                return;

            if (!_service.TryInstall(_selectedWeapon, _selectedModification))
                Refresh();
        }

        private void OnBuildChanged()
        {
            _selectedModification = null;
            Refresh();
        }

        private readonly struct StatDefinition
        {
            public StatDefinition(WeaponStat stat, string labelKey, bool isHigherBetter, string format, string suffix)
            {
                Stat = stat;
                LabelKey = labelKey;
                IsHigherBetter = isHigherBetter;
                Format = format;
                Suffix = suffix;
            }

            public WeaponStat Stat { get; }

            public string LabelKey { get; }

            public bool IsHigherBetter { get; }

            public string Format { get; }

            public string Suffix { get; }
        }
    }
}
