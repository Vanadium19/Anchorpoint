using UnityEngine;

namespace InventoryModule
{
    public class DropService : IDropService
    {
        private readonly Transform _playerTransform;
        private readonly float _dropDistance;
        private readonly float _dropOffsetY;
        private readonly IContainerWindowService _windowService;

        public DropService(
            Transform playerTransform,
            IContainerWindowService windowService,
            float dropDistance = 2f,
            float dropOffsetY = 0.5f)
        {
            _playerTransform = playerTransform;
            _windowService = windowService;
            _dropDistance = dropDistance;
            _dropOffsetY = dropOffsetY;
        }

        public bool CanDrop(ItemTable item)
        {
            if (item == null)
                return false;

            if (item.ItemDataSo == null)
                return false;

            return item.ItemDataSo.IsDropable;
        }

        public bool TryDropItem(ItemTable item)
        {
            if (!CanDrop(item))
                return false;

            LootItemView prefab = item.ItemDataSo.WorldPrefab;

            if (prefab == null)
                return false;

            if (item.IsContainer)
                _windowService.CloseAllWindowsForItem(item);

            Vector3 dropPosition = GetDropPosition();

            LootItemView lootInstance = Object.Instantiate(prefab, dropPosition, Quaternion.identity);
            lootInstance.SetItemTable(item);

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

            Vector3 forward = _playerTransform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 dropPos = _playerTransform.position + forward * _dropDistance;
            dropPos.y += _dropOffsetY;

            return dropPos;
        }
    }
}
