using ComponentsModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class ExplosionDamageService : IExplosionDamageService
    {
        private readonly PlayerProvider _playerProvider;

        public ExplosionDamageService(PlayerProvider playerProvider)
        {
            _playerProvider = playerProvider;
        }

        public void ApplyToPlayer(Vector3 center, float radius, float maxDamage)
        {
            if (radius <= 0f || maxDamage <= 0f)
                return;

            if (!_playerProvider.TryGet<IDamageable>(out var damageable) || !damageable.IsAlive)
                return;

            if (!_playerProvider.TryGet<Transform>(out var playerTransform))
                return;

            var distance = GetDistanceToPlayer(center, playerTransform);

            if (distance >= radius)
                return;

            var damage = maxDamage * (1f - distance / radius);
            damageable.TakeDamage(damage, playerTransform.position);
        }

        private static float GetDistanceToPlayer(Vector3 center, Transform playerTransform)
        {
            if (!playerTransform.TryGetComponent(out Collider playerCollider))
                return Vector3.Distance(center, playerTransform.position);

            return Vector3.Distance(center, playerCollider.bounds.ClosestPoint(center));
        }
    }
}
