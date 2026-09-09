using System;
using RandomEventsModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    /// <summary>Asset wrapper for <see cref="SpawnEnemiesAction"/>.</summary>
    [Serializable]
    public class SpawnEnemiesActionAsset : ZenjectRandomEventActionAsset<SpawnEnemiesAction>
    {
        [SerializeField] [Min(1)] private int enemyCount = 3;
        [SerializeField] [Min(0f)] private float spawnScatterRadius = 2f;

        /// <inheritdoc/>
        protected override object[] GetArguments(DiContainer container) => new object[] { enemyCount, spawnScatterRadius };
    }
}
