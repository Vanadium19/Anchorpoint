using System;
using UnityEngine;

namespace EnemyModule
{
    public sealed class GrenadeThrowComponent : IGrenadeThrowComponent
    {
        private const float GroundProbeHeight = 2f;
        private const float GroundProbeDepth = 6f;

        private readonly Transform _throwPoint;
        private readonly GameObject _grenadePrefab;
        private readonly GrenadierConfig _config;
        private readonly IExplosionDamageService _explosions;

        public GrenadeThrowComponent(Transform throwPoint,
            GameObject grenadePrefab,
            GrenadierConfig config,
            IExplosionDamageService explosions)
        {
            _throwPoint = throwPoint;
            _grenadePrefab = grenadePrefab;
            _config = config;
            _explosions = explosions;
        }

        public event Action ThrowStarted;
        public event Action ThrowReleased;

        public void BeginThrow() => ThrowStarted?.Invoke();

        public void Release(Vector3 targetPosition)
        {
            if (_throwPoint == null || _grenadePrefab == null)
                return;

            var grenadeObject = UnityEngine.Object.Instantiate(_grenadePrefab, _throwPoint.position, Quaternion.identity);

            if (!grenadeObject.TryGetComponent(out Grenade grenade))
            {
                UnityEngine.Object.Destroy(grenadeObject);
                return;
            }

            grenade.Launch(ProjectOntoGround(targetPosition), _config, _explosions, _throwPoint.root.gameObject);
            ThrowReleased?.Invoke();
        }

        private Vector3 ProjectOntoGround(Vector3 position)
        {
            var origin = position + Vector3.up * GroundProbeHeight;
            var distance = GroundProbeHeight + GroundProbeDepth;

            if (Physics.Raycast(origin, Vector3.down, out var hit, distance, _config.GroundMask, QueryTriggerInteraction.Ignore))
                return hit.point;

            return position;
        }
    }
}
