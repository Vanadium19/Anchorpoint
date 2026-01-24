using System;
using Zenject;
using EntityModule;
using PlayerModule.View;

namespace PlayerModule.Presenters
{
    public class PlayerHealthPresenter : IInitializable, IDisposable
    {
        private readonly HealthComponent _health;
        private readonly PlayerHUDView _view;

        public PlayerHealthPresenter(HealthComponent health, PlayerHUDView view)
        {
            _health = health;
            _view = view;
        }

        public void Initialize()
        {
            _health.HealthChanged += OnHealthChanged;
            UpdateView(_health.CurrentHealth, 100f);
        }

        public void Dispose()
        {
            _health.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float current, float max)
        {
            UpdateView(current, max);
        }

        private void UpdateView(float current, float max)
        {
            _view.SetHealth(current, max);
        }
    }
}