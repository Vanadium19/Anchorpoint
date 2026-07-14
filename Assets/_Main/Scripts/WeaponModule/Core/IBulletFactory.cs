using UnityEngine;

namespace WeaponModule
{
    public interface IBulletFactory
    {
        void SpawnBullet(GameObject bulletPrefab, Vector3 position, Quaternion rotation,
            float damage, float speed, float inheritFactor, Vector3 shooterVelocity);
    }
}


