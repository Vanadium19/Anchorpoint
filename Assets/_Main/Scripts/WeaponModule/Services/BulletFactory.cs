using UnityEngine;

namespace WeaponModule
{
    public class BulletFactory : IBulletFactory
    {
        public Bullet SpawnBullet(GameObject bulletPrefab, Vector3 position, Quaternion rotation,
            float damage, float speed, float inheritFactor, Vector3 shooterVelocity, GameObject owner,
            bool shouldPlayHitSound)
        {
            if (bulletPrefab == null)
                return null;

            var bulletObj = Object.Instantiate(bulletPrefab, position, rotation);

            if (!bulletObj.TryGetComponent(out Bullet bulletScript))
                return null;

            bulletScript.Setup(damage, speed, inheritFactor, shooterVelocity, owner);
            bulletScript.SetHitSoundEnabled(shouldPlayHitSound);
            return bulletScript;
        }
    }
}
