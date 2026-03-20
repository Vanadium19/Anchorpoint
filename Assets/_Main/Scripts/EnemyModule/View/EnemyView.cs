using ComponentsModule;
using UnityEngine;
using WeaponModule;
using Zenject;

namespace EnemyModule
{
    //TODO: Need refactor
    public class EnemyView : MonoBehaviour
    {
        private const float DestroyDelay = 10f;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyRagdoll ragdoll;

        private IHealthComponent _health;
        private IPathMoveComponent _movement;
        private IRangedAttackComponent _attack;

        private Vector3? _lastHitPosition;
        private Vector3? _lastHitForce;

        private bool _isHealthSubscribed;
        private bool _isAttackSubscribed;

        [Inject]
        public void Construct(IHealthComponent health, IPathMoveComponent movement, IRangedAttackComponent attack)
        {
            UnsubscribeFromHealth();
            UnsubscribeFromAttack();

            _health = health;
            _movement = movement;
            _attack = attack;

            SubscribeToHealth();
            SubscribeToAttack();
        }

        private void OnEnable()
        {
            SubscribeToHealth();
            SubscribeToAttack();
        }

        private void Update() => UpdateAnimator();

        private void OnDisable()
        {
            UnsubscribeFromHealth();
            UnsubscribeFromAttack();
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

        private void OnAttacked()
        {
            if (animator == null)
                return;

            animator.SetTrigger(ShootHash);
        }

        private void SubscribeToHealth()
        {
            if (_health == null || _isHealthSubscribed)
                return;

            _health.DamageTaken += OnDamageTaken;
            _health.Died += Die;
            _isHealthSubscribed = true;
        }

        private void UnsubscribeFromHealth()
        {
            if (_health == null || !_isHealthSubscribed)
                return;

            _health.DamageTaken -= OnDamageTaken;
            _health.Died -= Die;
            _isHealthSubscribed = false;
        }

        private void SubscribeToAttack()
        {
            if (_attack == null || _isAttackSubscribed)
                return;

            _attack.Attacked += OnAttacked;
            _isAttackSubscribed = true;
        }

        private void UnsubscribeFromAttack()
        {
            if (_attack == null || !_isAttackSubscribed)
                return;

            _attack.Attacked -= OnAttacked;
            _isAttackSubscribed = false;
        }
    }
}
