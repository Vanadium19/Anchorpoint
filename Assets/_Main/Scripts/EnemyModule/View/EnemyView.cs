using ComponentsModule;
using SharedData;
using UnityEngine;
using WeaponModule;
using Zenject;

namespace EnemyModule
{
    public class EnemyView : MonoBehaviour
    {
        private const float DestroyDelay = 10f;

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyRagdoll ragdoll;

        private IHealthComponent _health;
        private IPathMoveComponent _movement;
        private IRangedAttackComponent _attack;

        private Vector3? _lastHitPosition;
        private Vector3? _lastHitForce;

        [Inject]
        public void Construct(IHealthComponent health, IPathMoveComponent movement, IRangedAttackComponent attack)
        {
            _health = health;
            _movement = movement;
            _attack = attack;
        }

        private void OnEnable()
        {
            _health.DamageTaken += OnDamageTaken;
            _health.Died += Die;

            _attack.Attacked += OnAttacked;
        }

        private void Update() => UpdateAnimator();

        private void OnDisable()
        {
            _health.DamageTaken -= OnDamageTaken;
            _health.Died -= Die;

            _attack.Attacked -= OnAttacked;
        }

        private void UpdateAnimator() => animator.SetBool(EnemyAnimatorHashes.IsMoving, _movement.IsMoving);

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

        private void OnAttacked() => animator.SetTrigger(EnemyAnimatorHashes.Shoot);
    }
}