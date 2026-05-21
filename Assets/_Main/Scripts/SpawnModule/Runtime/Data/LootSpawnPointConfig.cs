using System.Collections.Generic;
using InventoryModule;
using UnityEngine;

namespace SpawnModule
{
    [CreateAssetMenu(menuName = "Game/Spawn/Loot Spawn Config", fileName = "NewLootSpawnConfig")]
    public class LootSpawnPointConfig : ScriptableObject
    {
        [SerializeField] private List<LootEntry> lootEntries = new List<LootEntry>();
        [SerializeField] private int minItemsToSpawn = 1;
        [SerializeField] private int maxItemsToSpawn = 1;
        [SerializeField] private float spawnRadius = 0f;

        public List<LootEntry> LootEntries => lootEntries;
        public int MinItemsToSpawn => minItemsToSpawn;
        public int MaxItemsToSpawn => maxItemsToSpawn;
        public float SpawnRadius => spawnRadius;

        public int GetRandomCount()
        {
            return minItemsToSpawn == maxItemsToSpawn 
                ? minItemsToSpawn 
                : Random.Range(minItemsToSpawn, maxItemsToSpawn + 1);
        }
    }
}
