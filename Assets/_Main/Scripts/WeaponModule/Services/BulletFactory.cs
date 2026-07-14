using UnityEngine;

namespace WeaponModule
{
    public class BulletFactory : IBulletFactory
    {
        public void SpawnBullet(GameObject bulletPrefab, Vector3 position, Quaternion rotation,
            float damage, float speed, float inheritFactor, Vector3 shooterVelocity)
        {
            if (bulletPrefab == null)
                return;

            var bulletObj = Object.Instantiate(bulletPrefab, position, rotation);

            if (bulletObj.TryGetComponent(out Bullet bulletScript))
                bulletScript.Setup(damage, speed, inheritFactor, shooterVelocity);
        }
    }
}
