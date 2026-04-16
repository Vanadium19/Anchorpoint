using System;
using UnityEngine;

namespace ComponentsModule
{
    public class HealthComponent : IHealthComponent
    {
        private readonly float _maxHealth;

        private float _currentHealth;

        public event Action<float, float> HealthChanged;
        public event Action<Vector3?, Vector3?> DamageTaken;
        public event Action Died;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsAlive => _currentHealth > 0;

        public HealthComponent(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null)
        {
            if (!IsAlive)
                return;

            _currentHealth -= amount;
            _currentHealth = Mathf.Max(_currentHealth, 0);

            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            DamageTaken?.Invoke(hitPoint, force);

            if (_currentHealth <= 0)
                Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0)
                return;

            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
}