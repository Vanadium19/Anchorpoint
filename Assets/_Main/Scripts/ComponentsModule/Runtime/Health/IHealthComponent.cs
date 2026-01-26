using System;
using UnityEngine;

namespace ComponentsModule
{
    public interface IHealthComponent
    {
        event Action<float, float> HealthChanged;
        event Action<Vector3?, Vector3?> DamageTaken;
        event Action Died;

        float MaxHealth { get; }
        float CurrentHealth { get; }
        bool IsAlive { get; }

        void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null);
    }
}