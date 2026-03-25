using System.Collections.Generic;
using UnityEngine;

namespace SpawnModule
{
    public class LevelSpawnPointsView : MonoBehaviour
    {
        [SerializeField] private List<SpawnPointView> spawnPoints;
        
        public List<SpawnPointView> SpawnPoints => spawnPoints;
    }
}