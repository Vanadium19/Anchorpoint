using System.Collections.Generic;
using UnityEngine;

namespace SpawnModule
{
    public class LevelLootSpawnPointsView : MonoBehaviour
    {
        [SerializeField] private List<LootSpawnPointView> spawnPoints = new List<LootSpawnPointView>();

        public List<LootSpawnPointView> SpawnPoints => spawnPoints;
    }
}
