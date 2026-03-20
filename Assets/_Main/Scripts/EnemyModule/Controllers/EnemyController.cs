using System;
using ComponentsModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    public sealed class EnemyController : IInitializable, ITickable, IDisposable
    {
        private readonly EnemyConfig _config;
        private readonly Enemy _model;
        private readonly EnemyView _view;
        private readonly AIAgent _aiAgent;
        private readonly IHealthComponent _health;
        private readonly IPathMoveComponent _movement;

        private Vector3? _lastHitPosition;
        private Vector3? _lastHitForce;

        public EnemyController(EnemyConfig config,
            Enemy model,
            EnemyView view,
            AIAgent aiAgent,
            IHealthComponent health,
            IPathMoveComponent movement)
        {
            _config = config;
            _model = model;
            _view = view;
            _aiAgent = aiAgent;
            _health = health;
            _movement = movement;
        }

        public void Initialize()
        {
            _lastHitPosition = null;
            _lastHitForce = null;

            _model.Initialize(_config.MaxAmmo, _view.transform.position);
            _aiAgent.Initialize();

            _health.DamageTaken += OnTakeDamage;
            _health.Died += OnDeath;
        }

        public void Tick()
        {
            if (!_health.IsAlive)
                return;

            _view.UpdateAnimator(_movement.Velocity);
            _aiAgent.Tick();
        }

        public void Dispose()
        {
            _health.DamageTaken -= OnTakeDamage;
            _health.Died -= OnDeath;
            _aiAgent.Dispose();
        }

        private void OnTakeDamage(Vector3? hitPos, Vector3? force)
        {
            _lastHitPosition = hitPos;
            _lastHitForce = force;
            _aiAgent.OnTakeDamage(hitPos, force);
        }

        private void OnDeath() => _view.Die(_lastHitPosition, _lastHitForce);
    }
}
