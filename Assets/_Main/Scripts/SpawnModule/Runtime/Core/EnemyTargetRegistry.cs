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

            _targets.Add(new EnemyCombatTarget(enemyObject));
        }

        public bool TryGetNearest(Vector3 origin, float radius, out ICombatTarget target)
        {
            target = null;

            var radiusSquared = radius * radius;
            var nearestSquaredDistance = float.MaxValue;

            for (var index = _targets.Count - 1; index >= 0; index--)
            {
                var candidate = _targets[index];

                if (!candidate.IsAlive)
                {
                    _targets.RemoveAt(index);
                    continue;
                }

                var squaredDistance = (candidate.Position - origin).sqrMagnitude;

                if (squaredDistance > radiusSquared)
                    continue;

                if (squaredDistance >= nearestSquaredDistance)
                    continue;

                nearestSquaredDistance = squaredDistance;
                target = candidate;
            }

            return target != null;
        }
    }
}
