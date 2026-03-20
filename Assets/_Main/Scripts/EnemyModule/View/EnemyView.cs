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

        [Inject]
        public void Construct(IHealthComponent health, IPathMoveComponent movement)
        {
            _health = health;
            _movement = movement;
            _lastHitPosition = null;
            _lastHitForce = null;
        }

        private void OnEnable()
        {
            _health.DamageTaken += OnDamageTaken;
            _health.Died += Die;
        }

        private void Update() => UpdateAnimator();

        private void OnDisable()
        {
            _health.DamageTaken -= OnDamageTaken;
            _health.Died -= Die;
        }

        //TODO: Вынести пули в модуль оружия
        public void SpawnBullet(Vector3 targetPos, float damage, float bulletSpeed)
        {
            animator.SetTrigger(ShootHash);
            var direction = (targetPos - firePoint.position).normalized;

            var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            bullet.Setup(damage, bulletSpeed, 0f, Vector3.zero);
        }

        private void UpdateAnimator() => animator.SetBool(IsMovingHash, _movement.IsMoving);

        private void Die()
        {
            ragdoll.Activate(_lastHitForce, _lastHitPosition);
            Destroy(gameObject, DestroyDelay);
        }

        private void OnDamageTaken(Vector3? hitPosition, Vector3? hitForce)
        {
            _lastHitPosition = hitPosition;
            _lastHitForce = hitForce;
        }
    }
}