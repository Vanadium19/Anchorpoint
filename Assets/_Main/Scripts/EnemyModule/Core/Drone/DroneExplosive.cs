using System;
using ComponentsModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    public sealed class DroneExplosive : IDroneExplosive, IInitializable, IDisposable
    {
        private readonly DroneConfig _config;
        private readonly IHealthComponent _health;
        private readonly IExplosionDamageService _explosions;
        private readonly Transform _transform;

        private bool _hasDetonated;

        public DroneExplosive(DroneConfig config,
            IHealthComponent health,
            IExplosionDamageService explosions,
            Transform transform)
        {
            _config = config;
            _health = health;
            _explosions = explosions;
            _transform = transform;
        }

        public event Action FuseLit;
        public event Action<Vector3, float> Detonated;

        public bool IsFuseLit { get; private set; }

        public void Initialize() => _health.Died += OnDied;

        public void Dispose() => _health.Died -= OnDied;

        public void LightFuse()
        {
            if (IsFuseLit || _hasDetonated)
                return;

            IsFuseLit = true;
            FuseLit?.Invoke();
        }

        public void Detonate() => Explode(_config.ExplosionRadius);

        private void OnDied() => Explode(_config.ShotDownExplosionRadius);

        private void Explode(float radius)
        {
            if (_hasDetonated)
                return;

            _hasDetonated = true;

            var position = _transform.position;
            _explosions.ApplyToPlayer(position, radius, _config.ExplosionDamage);
            Detonated?.Invoke(position, radius);
        }
    }
}
