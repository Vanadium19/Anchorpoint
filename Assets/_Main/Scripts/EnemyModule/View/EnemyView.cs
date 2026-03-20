using ComponentsModule;
using UnityEngine;
using UnityEngine.AI;
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
        [SerializeField] private Transform eyes;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyRagdoll ragdoll;
        [SerializeField] private Transform[] patrolPoints;

        [SerializeField] private NavMeshAgent agent;

        private IHealthComponent _health;

        public Transform[] PatrolPoints => patrolPoints;
        public IHealthComponent Health => _health;
        public Transform Eyes => eyes;
        public Transform FirePoint => firePoint;
        public NavMeshAgent Agent => agent;

        [Inject]
        public void Construct(IHealthComponent health)
        {
            _health = health;
        }

        private void Awake()
        {
            agent ??= GetComponent<NavMeshAgent>();
        }

        private void OnValidate()
        {
            agent ??= GetComponent<NavMeshAgent>();
        }

        public void UpdateAnimator(Vector3 velocity)
        {
            var isMoving = velocity.sqrMagnitude > 0.01f;
            animator.SetBool(IsMovingHash, isMoving);
        }

        //TODO: Вынести пули в модуль оружия
        public void SpawnBullet(Vector3 targetPos, float damage, float bulletSpeed)
        {
            animator.SetTrigger(ShootHash);

            var direction = (targetPos - firePoint.position).normalized;

            var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
            bullet.Setup(damage, bulletSpeed, 0f, Vector3.zero);
        }

        public void Die(Vector3? hitPosition, Vector3? hitForce)
        {
            ragdoll.Activate(hitForce, hitPosition);
            Destroy(gameObject, DestroyDelay);
        }
    }
}