using System;
using UnityEngine;

namespace ComponentsModule
{
    public interface IHealthComponent
    {
        event Action<float, float> HealthChanged;
        event Action<Vector3?, Vector3?> DamageTook;
        event Action Died;

        float MaxHealth { get; }
        float CurrentHealth { get; }
    }
}