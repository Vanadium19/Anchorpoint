using UnityEngine;

namespace ComponentsModule
{
    public interface IProjectile
    {
        void Setup(float damage, float projectileSpeed, float inheritFactor, Vector3 shooterVelocity);
    }
}
