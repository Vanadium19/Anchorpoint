using UnityEngine;

namespace BuildingModule
{
    /// <summary>Health of a single placed building.</summary>
    /// <remarks>A building with no health left counts as broken and stays broken until <see cref="Restore"/> is called.</remarks>
    public class BuildingHealth
    {
        private readonly float _maxHealth;

        private float _currentHealth;

        /// <summary>Creates full health from the building's maximum.</summary>
        public BuildingHealth(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        /// <summary>Health the building has when intact.</summary>
        public float MaxHealth => _maxHealth;

        /// <summary>Health the building has left.</summary>
        public float CurrentHealth => _currentHealth;

        /// <summary>Whether the building has run out of health.</summary>
        public bool IsBroken => _currentHealth <= 0f;

        /// <summary>Subtracts damage and returns whether this call is the one that broke the building.</summary>
        public bool TakeDamage(float amount)
        {
            if (IsBroken || amount <= 0f)
                return false;

            _currentHealth = Mathf.Max(_currentHealth - amount, 0f);

            return IsBroken;
        }

        /// <summary>Returns the building to full health.</summary>
        public void Restore() => _currentHealth = _maxHealth;
    }
}
