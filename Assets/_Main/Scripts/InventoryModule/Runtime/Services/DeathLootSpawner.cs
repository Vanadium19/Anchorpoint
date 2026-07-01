using UnityEngine;
using UnityEngine.SceneManagement;

namespace InventoryModule
{
    public class DeathLootSpawner : IDeathLootSpawner
    {
        private readonly IDropService _dropService;
        private readonly IDeathLootStorage _storage;
        private readonly float _scatterRadius;
        private readonly float _spawnOffsetY;

        public DeathLootSpawner(
            IDropService dropService,
            IDeathLootStorage storage,
            float scatterRadius = 1.25f,
            float spawnOffsetY = 0.35f)
        {
            _dropService = dropService;
            _storage = storage;
            _scatterRadius = scatterRadius;
            _spawnOffsetY = spawnOffsetY;
        }

        public void SpawnStoredPiles()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            var piles = _storage.GetPiles(sceneName);

            for (var i = 0; i < piles.Count; i++)
                SpawnPile(piles[i]);
        }

        public void SpawnPile(DeathLootPileData pile)
        {
            if (pile == null)
                return;

            var items = pile.Items;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (item == null || item.ItemDataSo == null || !item.ItemDataSo.IsDropable)
                    continue;

                var spawnPosition = GetSpawnPosition(pile.Position);
                _dropService.TryDropItem(item, spawnPosition);
            }
        }

        private Vector3 GetSpawnPosition(Vector3 centerPosition)
        {
            var offset = Random.insideUnitCircle * _scatterRadius;

            return new Vector3(
                centerPosition.x + offset.x,
                centerPosition.y + _spawnOffsetY,
                centerPosition.z + offset.y);
        }
    }
}
