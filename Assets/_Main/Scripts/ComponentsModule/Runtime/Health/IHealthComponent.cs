using System;
using UnityEngine;

namespace ComponentsModule
{
    public interface IHealthComponent : IDamageable
    {
        event Action<float, float> HealthChanged;
        event Action<Vector3?, Vector3?> DamageTaken;
        event Action Died;

        float MaxHealth { get; }
        float CurrentHealth { get; }

        void Heal(float amount);
    }
}