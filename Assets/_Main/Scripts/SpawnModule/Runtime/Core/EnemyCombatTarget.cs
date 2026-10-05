using ComponentsModule;
using UnityEngine;

namespace SpawnModule
{
    public sealed class EnemyCombatTarget : ICombatTarget
    {
        private readonly GameObject _enemyObject;

        private IDamageable _damageable;

        public EnemyCombatTarget(GameObject enemyObject)
        {
            _enemyObject = enemyObject;
        }

        public Vector3 Position =>
            _enemyObject != null
                ? _enemyObject.transform.position
                : Vector3.zero;

        public bool IsAlive =>
            _enemyObject != null &&
            (!TryGetDamageable(out var damageable) || damageable.IsAlive);

        public void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null)
        {
            if (!TryGetDamageable(out var damageable))
                return;

            damageable.TakeDamage(amount, hitPoint, force);
        }

        private bool TryGetDamageable(out IDamageable damageable)
        {
            if (_damageable == null && _enemyObject != null)
            {
                var entity = _enemyObject.GetComponentInChildren<IEntity>();

                if (entity != null)
                    entity.TryGet(out _damageable);
            }

            damageable = _damageable;
            return damageable != null;
        }
    }
}
