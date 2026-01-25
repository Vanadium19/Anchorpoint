using System;
using UnityEngine;

namespace ComponentsModule
{
    //TODO: Переделать из MonoBehaviour
    public class HealthComponent : MonoBehaviour, IDamageable, IHealthComponent
    {
        [SerializeField] private float maxHealth = 100f;

        private float _currentHealth;

        public event Action<float, float> HealthChanged;
        public event Action<Vector3?, Vector3?> DamageTook;
        public event Action Died;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsAlive => _currentHealth > 0;

        private void Awake() => _currentHealth = maxHealth;

        public void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null)
        {
            if (!IsAlive)
                return;

            _currentHealth -= amount;
            _currentHealth = Mathf.Max(_currentHealth, 0);

            HealthChanged?.Invoke(_currentHealth, maxHealth);
            DamageTook?.Invoke(hitPoint, force);

            if (_currentHealth <= 0)
                Die();
        }

        //FIXME: Если нет задачи, не надо делать лишние методы
        public void Heal(float amount)
        {
            if (!IsAlive)
                return;

            _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
            HealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void Die() => Died?.Invoke();
    }
}