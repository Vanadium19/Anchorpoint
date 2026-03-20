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

        public EnemyController(EnemyConfig config,
            Enemy model,
            EnemyView view,
            AIAgent aiAgent,
            IHealthComponent health)
        {
            _config = config;
            _model = model;
            _view = view;
            _aiAgent = aiAgent;
            _health = health;
        }

        public void Initialize()
        {
            _model.Initialize(_config.MaxAmmo, _view.transform.position);
            _aiAgent.Initialize();
            _health.DamageTaken += OnTakeDamage;
        }

        public void Tick() => _aiAgent.Tick();

        public void Dispose()
        {
            _health.DamageTaken -= OnTakeDamage;
            _aiAgent.Dispose();
        }

        private void OnTakeDamage(Vector3? hitPos, Vector3? force) => _aiAgent.OnTakeDamage(hitPos, force);
    }
}
