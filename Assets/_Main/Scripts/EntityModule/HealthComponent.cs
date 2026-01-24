using System;
using UnityEngine;

namespace EntityModule
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        private float _currentHealth;

        public event Action<float, float> HealthChanged;
        public event Action<Vector3?, Vector3?> TookDamage;
        public event Action Died;

        public float CurrentHealth => _currentHealth;
        public bool IsAlive => _currentHealth > 0;

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null)
        {
            if (!IsAlive) return;

            _currentHealth -= amount;
            Debug.Log($"[Health] {gameObject.name} получил {amount} урона. HP: {_currentHealth}/{maxHealth}");
            HealthChanged?.Invoke(_currentHealth, maxHealth);
            TookDamage?.Invoke(hitPoint, force);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
            HealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void Die()
        {
            Died?.Invoke();
        }
    }
}