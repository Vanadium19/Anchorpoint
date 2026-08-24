using UnityEngine;

namespace InventoryModule
{
    public class DropService : IDropService
    {
        private readonly Transform _playerTransform;
        private readonly float _dropDistance;
        private readonly float _dropOffsetY;
        private readonly IContainerWindowService _windowService;
        private readonly LootViewFactory _lootViewFactory;

        public DropService(
            Transform playerTransform,
            IContainerWindowService windowService,
            LootViewFactory lootViewFactory,
            float dropDistance = 2f,
            float dropOffsetY = 0.5f)
        {
            _playerTransform = playerTransform;
            _windowService = windowService;
            _lootViewFactory = lootViewFactory;
            _dropDistance = dropDistance;
            _dropOffsetY = dropOffsetY;
        }

        public bool CanDrop(ItemTable item)
        {
            if (item == null)
                return false;

            return item.ItemDataSo != null && item.ItemDataSo.IsDropable;
        }

        public bool TryDropItem(ItemTable item)
        {
            var dropPosition = GetDropPosition();
            return TryDropItem(item, dropPosition);
        }

        public bool TryDropItem(ItemTable item, Vector3 worldPosition) =>
            TryDropItem(item, worldPosition, false);

        public bool TryDropItem(
            ItemTable item,
            Vector3 worldPosition,
            bool isPlayerDeathLoot)
        {
            if (!CanDrop(item))
                return false;

            var prefab = item.ItemDataSo.WorldPrefab;

            if (prefab == null)
                return false;

            if (item.IsContainer)
                _windowService.CloseAllWindowsForItem(item);

            var lootInstance = _lootViewFactory != null
                ? _lootViewFactory.Create(prefab, worldPosition)
                : Object.Instantiate(prefab, worldPosition, Quaternion.identity);

            lootInstance.SetItemTable(item);
            lootInstance.SetPlayerDeathLoot(isPlayerDeathLoot);

            return true;
        }

        private Vector3 GetDropPosition()
        {
            if (_playerTransform == null)
            {
                var camera = Camera.main;

                if (camera != null)
                    return camera.transform.position + camera.transform.forward * _dropDistance;

                return Vector3.zero;
            }

            var forward = _playerTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            var dropPosition = _playerTransform.position + forward * _dropDistance;
            dropPosition.y += _dropOffsetY;

            return dropPosition;
        }
    }
}
