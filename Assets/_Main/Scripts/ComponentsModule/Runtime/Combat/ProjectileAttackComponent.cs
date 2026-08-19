using System;
using UnityEngine;

namespace ComponentsModule
{
    public sealed class ProjectileAttackComponent : IRangedAttackComponent
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly Transform _firePoint;
        private readonly GameObject _projectilePrefab;

        private int _currentAmmo;
        private int _maxAmmo;

        public ProjectileAttackComponent(Transform firePoint, GameObject projectilePrefab, int maxAmmo)
        {
            _firePoint = firePoint;
            _projectilePrefab = projectilePrefab;
            
            _maxAmmo = Mathf.Max(0, maxAmmo);
            _currentAmmo = _maxAmmo;
        }

        public event Action Attacked;

        public int CurrentAmmo => _currentAmmo;
        public bool IsEmpty => _currentAmmo <= 0;

        public bool TryAttack(Vector3 targetPosition, float damage, float projectileSpeed)
        {
            if (IsEmpty || _firePoint == null || _projectilePrefab == null)
                return false;

            Vector3 direction = targetPosition - _firePoint.position;

            if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                direction = _firePoint.forward;

            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            GameObject projectileObject = GameObject.Instantiate(_projectilePrefab, _firePoint.position, rotation);

            if (!projectileObject.TryGetComponent(out IProjectile projectile))
            {
                GameObject.Destroy(projectileObject);
                return false;
            }

            _currentAmmo--;
            projectile.Setup(damage, projectileSpeed, 0f, Vector3.zero, _firePoint.root.gameObject);
            Attacked?.Invoke();
            return true;
        }

        public void Reload()
        {
            _currentAmmo = _maxAmmo;
        }
    }
}
