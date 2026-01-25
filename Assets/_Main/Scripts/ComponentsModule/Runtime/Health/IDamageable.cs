using UnityEngine;

namespace ComponentsModule
{
    public interface IDamageable
    {
        bool IsAlive { get; }

        void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null);
    }
}