using ComponentsModule;
using UnityEngine;
using WeaponModule;
using Zenject;

namespace EnemyModule
{
    public class EnemyView : MonoBehaviour
    {
        private const float DestroyDelay = 10f;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");

        [Header("References")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyRagdoll ragdoll;

        private IHealthComponent _health;
        private IPathMoveComponent _movement;

        private Vector3? _lastHitPosition;
        private Vector3? _lastHitForce;
        private bool _isSubscribed;

        [Inject]
        public void Construct(IHealthComponent health, IPathMoveComponent movement)
        {
            UnsubscribeFromHealth();

            _health = health;
            _movement = movement;
            _lastHitPosition = null;
            _lastHitForce = null;
            _isSubscribed = false;

            SubscribeToHealth();
        }

        private void OnEnable()
        {
            SubscribeToHealth();
        }

        private void Update() => UpdateAnimator();

        private void OnDisable()
        {
            UnsubscribeFromHealth();
        }

        //TODO: Вынести пули в модуль оружия
        public void SpawnBullet(Vector3 position, float damage, float bulletSpeed)
        {
            if (animator != null)
                animator.SetTrigger(ShootHash);
            var direction = (position - firePoint.position).normalized;

            var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            bullet.Setup(damage, bulletSpeed, 0f, Vector3.zero);
        }

        private void UpdateAnimator()
        {
            if (animator == null || _movement == null)
                return;

            animator.SetBool(IsMovingHash, _movement.IsMoving);
        }

        private void Die()
        {
            if (ragdoll == null)
            {
                Destroy(gameObject);
                return;
            }

            ragdoll.Activate(_lastHitForce, _lastHitPosition);
            Destroy(gameObject, DestroyDelay);
        }

        private void OnDamageTaken(Vector3? hitPosition, Vector3? hitForce)
        {
            _lastHitPosition = hitPosition;
            _lastHitForce = hitForce;
        }

        private void SubscribeToHealth()
        {
            if (_health == null || _isSubscribed)
                return;

            _health.DamageTaken += OnDamageTaken;
            _health.Died += Die;
            _isSubscribed = true;
        }

        private void UnsubscribeFromHealth()
        {
            if (_health == null || !_isSubscribed)
                return;

            _health.DamageTaken -= OnDamageTaken;
            _health.Died -= Die;
            _isSubscribed = false;
        }
    }
}
