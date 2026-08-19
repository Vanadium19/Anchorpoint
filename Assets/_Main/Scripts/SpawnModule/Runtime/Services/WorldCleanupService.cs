using EnemyModule;
using InventoryModule;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpawnModule
{
    public class WorldCleanupService
    {
        public void RemoveTransientObjects(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            var rootObjects = scene.GetRootGameObjects();

            for (var rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                RemoveEnemies(rootObjects[rootIndex]);
                RemoveRegularLoot(rootObjects[rootIndex]);
            }
        }

        private static void RemoveEnemies(GameObject rootObject)
        {
            var enemies = rootObject.GetComponentsInChildren<EnemyView>(true);

            for (var enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
            {
                var enemy = enemies[enemyIndex];

                if (enemy != null)
                    Object.Destroy(enemy.gameObject);
            }
        }

        private static void RemoveRegularLoot(GameObject rootObject)
        {
            var lootItems = rootObject.GetComponentsInChildren<LootItemView>(true);

            for (var lootIndex = 0; lootIndex < lootItems.Length; lootIndex++)
            {
                var lootItem = lootItems[lootIndex];

                if (lootItem == null || lootItem.IsPlayerDeathLoot)
                    continue;

                Object.Destroy(lootItem.gameObject);
            }
        }
    }
}
