using System.Collections.Generic;
using InventoryModule;
using UnityEngine;

namespace SpawnModule
{
    public class LootSpawnPointView : MonoBehaviour
    {
        [SerializeField] private LootSpawnPointConfig config;
        [SerializeField] private bool overrideConfig;
        [SerializeField] private List<LootEntry> lootEntries = new List<LootEntry>();
        [SerializeField] private int minItemsToSpawn = 1;
        [SerializeField] private int maxItemsToSpawn = 1;
        [SerializeField] private float spawnRadius = 0f;

        private LootSpawnPointConfig _activeConfig;

        private void Awake()
        {
            _activeConfig = overrideConfig ? null : config;
        }

        public LootSpawnPointConfig GetConfig()
        {
            if (_activeConfig != null)
                return _activeConfig;

            var tempConfig = ScriptableObject.CreateInstance<LootSpawnPointConfig>();
            return tempConfig;
        }

        public List<LootEntry> GetLootEntries()
        {
            return overrideConfig ? lootEntries : config?.LootEntries ?? lootEntries;
        }

        public int GetMinItemsToSpawn()
        {
            return overrideConfig ? minItemsToSpawn : config?.MinItemsToSpawn ?? minItemsToSpawn;
        }

        public int GetMaxItemsToSpawn()
        {
            return overrideConfig ? maxItemsToSpawn : config?.MaxItemsToSpawn ?? maxItemsToSpawn;
        }

        public float GetSpawnRadius()
        {
            return overrideConfig ? spawnRadius : config?.SpawnRadius ?? spawnRadius;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            float radius = overrideConfig ? spawnRadius : (config?.SpawnRadius ?? spawnRadius);
            
            if (radius > 0)
            {
                Gizmos.color = new Color(1, 0.6f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, radius);
            }
        }
#endif
    }
}
