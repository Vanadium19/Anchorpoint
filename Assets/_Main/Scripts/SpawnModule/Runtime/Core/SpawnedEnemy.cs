using ComponentsModule;
using UnityEngine;

namespace SpawnModule
{
    /// <summary>Wraps a spawned enemy and answers whether it is still alive.</summary>
    public class SpawnedEnemy
    {
        private readonly GameObject _gameObject;

        private IHealthComponent _health;

        /// <summary>Creates the wrapper over a spawned enemy object.</summary>
        public SpawnedEnemy(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        /// <summary>Whether the enemy is still alive.</summary>
        /// <remarks>
        /// An enemy whose health component cannot be resolved yet counts as alive, so a group is never
        /// considered cleared before its enemies finish initializing.
        /// </remarks>
        public bool IsAlive => _gameObject != null && (!TryGetHealth(out var health) || health.IsAlive);

        private bool TryGetHealth(out IHealthComponent health)
        {
            if (_health == null)
            {
                var entity = _gameObject.GetComponentInChildren<IEntity>();

                if (entity != null)
                    entity.TryGet(out _health);
            }

            health = _health;

            return health != null;
        }
    }
}
