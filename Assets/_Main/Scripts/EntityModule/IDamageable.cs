using UnityEngine;

namespace EntityModule
{
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null);
        bool IsAlive { get; }
    }
}