 using System.Collections.Generic;
using UnityEngine;
using Zenject;
using InventoryModule.ContextMenu.Configs;
using InventoryModule.ContextMenu.Actions;
using InventoryModule.ContextMenu.Presets;

namespace InventoryModule.ContextMenu
{
    public interface IContextActionService
    {
        IReadOnlyList<IContextAction> GetActions(ItemTable item);
        void Initialize(IDropService dropService, ContainerWindow containerWindowPrefab, AbstractGrid gridPrefab, Canvas canvas);
    }

    public class ContextActionService : IContextActionService
    {
        private static IContextActionService _cachedInstance;
        private static bool _isInitialized;

        public static IContextActionService CachedInstance
        {
            get => _cachedInstance;
        }

        public static bool IsInitialized => _isInitialized;

        public static void SetInitialized()
        {
            _isInitialized = true;
        }

        private readonly IEquipmentSlotService _slotService;
        private IDropService _dropService;
        private ContainerWindow _containerWindowPrefab;
        private AbstractGrid _gridPrefab;
        private Canvas _canvas;

        public ContextActionService(
            IEquipmentSlotService slotService)
        {
            _slotService = slotService;
        }

        public void Initialize(
            IDropService dropService,
            ContainerWindow containerWindowPrefab,
            AbstractGrid gridPrefab,
            Canvas canvas)
        {
            _dropService = dropService;
            _containerWindowPrefab = containerWindowPrefab;
            _gridPrefab = gridPrefab;
            _canvas = canvas;

            _cachedInstance = this;
            _isInitialized = true;
        }

        public IReadOnlyList<IContextAction> GetActions(ItemTable item)
        {
            var actions = new List<IContextAction>();

            var preset = item.ItemDataSo.ContextActionPreset;
            if (preset == null)
            {
                return actions;
            }

            var sortedConfigs = preset.GetSortedConfigs();

            foreach (var config in sortedConfigs)
            {
                var action = CreateActionFromConfig(config, item);
                if (action == null)
                {
                    continue;
                }

                if (!action.IsAvailable)
                {
                    continue;
                }

                actions.Add(action);
            }

            return actions;
        }

        private IContextAction CreateActionFromConfig(ActionConfigBase config, ItemTable item)
        {
            var displayName = config.GetDisplayName();

            switch (config)
            {
                case EquipActionConfig _:
                    return new EquipAction(item, displayName, _slotService);

                case DropActionConfig _:
                    if (!item.ItemDataSo.IsDropable)
                        return null;
                    return new DropAction(item, displayName, _dropService, _slotService);

                case OpenActionConfig _:
                    return new OpenAction(item, displayName, _containerWindowPrefab, _gridPrefab, _canvas);

                case SplitActionConfig splitConfig:
                    if (!item.IsStackable || item.StackCount < splitConfig.MinStackCount)
                        return null;
                    return new SplitAction(item, displayName);

                case UseActionConfig _:
                    return new UseAction(item, displayName);

                case InspectActionConfig _:
                    return new InspectAction(item, displayName);

                default:
                    return null;
            }
        }
    }
}
