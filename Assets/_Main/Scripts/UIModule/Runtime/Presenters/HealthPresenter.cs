using System;
using Zenject;
using ComponentsModule;

namespace UIModule
{
    public class HealthPresenter : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly HealthView _view;

        public HealthPresenter(IHealthComponent health, HealthView view)
        {
            _health = health;
            _view = view;
        }

        public void Initialize()
        {
            OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
            _health.HealthChanged += OnHealthChanged;
        }

        public void Dispose() => _health.HealthChanged -= OnHealthChanged;

        private void OnHealthChanged(float current, float max) => _view.SetHealth(current, max);
    }
}