using System;
using Zenject;
using ComponentsModule;

namespace UIModule
{
    public class HealthPresenter : IInitializable, IDisposable
    {
        private readonly IHealthComponent _health;
        private readonly HealthView _view;

        public HealthPresenter(IHealthComponent health, [InjectOptional] HealthView view)
        {
            _health = health;
            _view = view;
        }

        public void Initialize()
        {
            if (_view == null)
                return;
                
            OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
            _health.HealthChanged += OnHealthChanged;
        }

        public void Dispose()
        {
            if (_view != null && _health != null)
                _health.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float current, float max)
        {
            if (_view != null)
                _view.SetHealth(current, max);
        }
    }
}