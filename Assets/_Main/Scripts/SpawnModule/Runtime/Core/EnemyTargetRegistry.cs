using System;
using System.Collections.Generic;
using ComponentsModule;
using UnityEngine;

namespace SpawnModule
{
    public sealed class EnemyTargetRegistry : ICombatTargetSource
    {
        private readonly List<EnemyCombatTarget> _targets = new();

        public void Register(GameObject enemyObject)
        {
            if (enemyObject == null)
                return;

            RemoveDead();

            var target = new EnemyCombatTarget(enemyObject);
            _targets.Add(target);
            SubscribeDeath(enemyObject, target);
        }

        public bool TryGetNearest(Vector3 origin, float radius, out ICombatTarget target)
        {
            target = null;
            RemoveDead();

            var radiusSquared = radius * radius;
            var nearestSquaredDistance = float.MaxValue;

            for (var index = 0; index < _targets.Count; index++)
            {
                var candidate = _targets[index];
                var squaredDistance = (candidate.Position - origin).sqrMagnitude;

                if (squaredDistance > radiusSquared || squaredDistance >= nearestSquaredDistance)
                    continue;

                nearestSquaredDistance = squaredDistance;
                target = candidate;
            }

            return target != null;
        }

        private void SubscribeDeath(GameObject enemyObject, EnemyCombatTarget target)
        {
            var entity = enemyObject.GetComponentInChildren<IEntity>();

            if (entity == null || !entity.TryGet(out IHealthComponent health))
                return;

            Action onDied = null;
            onDied = () =>
            {
                health.Died -= onDied;
                _targets.Remove(target);
            };

            health.Died += onDied;
        }

        private void RemoveDead()
        {
            for (var index = _targets.Count - 1; index >= 0; index--)
            {
                if (!_targets[index].IsAlive)
                    _targets.RemoveAt(index);
            }
        }
    }
}
