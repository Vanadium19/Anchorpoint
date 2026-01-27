using ComponentsModule;
using UnityEngine;
using UnityEngine.AI;
using WeaponModule;
using Zenject;

namespace EnemyModule
{
    public class EnemyView : MonoBehaviour
    {
        [Header("References")] [SerializeField] private Transform eyes;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyRagdoll ragdoll;
        [SerializeField] private Transform[] patrolPoints;

        [SerializeField] private NavMeshAgent agent;

        private IHealthComponent _health;
        private EnemyConfig _config;

        public Transform[] PatrolPoints => patrolPoints;
        public IHealthComponent Health => _health;
        public Transform Eyes => eyes;
        public Transform FirePoint => firePoint;

        public bool IsPathPending => agent.pathPending;
        public float RemainingDistance => agent.remainingDistance;
        public Vector3 Velocity => agent != null ? agent.velocity : Vector3.zero;

        [Inject]
        public void Construct(IHealthComponent health)
        {
            _health = health;
        }

        public void Initialize(EnemyConfig config)
        {
            _config = config;
            agent.stoppingDistance = 0.5f;
        }

        public void MoveTo(Vector3 position)
        {
            if (agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(position);
            }
        }

        public void StopMove()
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        //FIXME: Unused method
        public void ResetPath()
        {
            if (agent.enabled)
                agent.ResetPath();
        }

        public void RotateTowards(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;

            if (direction == Vector3.zero)
                return;

            var targetRotation = Quaternion.LookRotation(direction);

            //FIXME: Magic numbers
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        public void UpdateAnimator(Vector3 velocity)
        {
            if (animator)
            {
                bool isMoving = velocity.sqrMagnitude > 0.01f;
                //TODO: Вынести в класс с константами для аниматора и сделать через хэш
                animator.SetBool("IsMoving", isMoving);
            }
        }

        public void SpawnBullet(Vector3 targetPos)
        {
            //TODO: Вынести в класс с константами для аниматора и сделать через хэш
            if (animator)
                animator.SetTrigger("Shoot");

            Vector3 dir = (targetPos - firePoint.position).normalized;
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));

            if (bulletObj.TryGetComponent(out Bullet bullet))
                bullet.Setup(_config.Damage, _config.BulletSpeed, 0f, Vector3.zero);
        }

        public void Die(Vector3? hitPos, Vector3? hitForce)
        {
            if (ragdoll)
                ragdoll.Activate(hitForce, hitPos);
            else
                Destroy(gameObject);

            //FIXME: Magic numbers
            if (ragdoll)
                Destroy(gameObject, 10f);
        }

        public bool CheckLineOfSight(Transform target, float range, float angle, LayerMask mask, out Vector3 hitPoint)
        {
            hitPoint = target.position;

            if (target == null)
                return false;

            Vector3 targetCenter;

            //FIXME: Magic numbers
            if (target.TryGetComponent(out Collider targetCol))
                targetCenter = targetCol.bounds.center;
            else
                targetCenter = target.position + Vector3.up * 1.0f;

            Debug.DrawLine(eyes.position, targetCenter, Color.yellow);
            float dist = Vector3.Distance(eyes.position, targetCenter);
            Vector3 dir = (targetCenter - eyes.position).normalized;
            float angleToTarget = Vector3.Angle(eyes.forward, dir);

            if (dist > range)
                return false;

            //FIXME: Magic numbers
            if (angleToTarget > angle / 2)
                return false;

            if (Physics.Raycast(eyes.position, dir, out RaycastHit hit, range, mask))
            {
                if (hit.transform.root == target.root)
                {
                    hitPoint = targetCenter;
                    return true;
                }
            }

            return false;
        }

        public bool FindCover(Vector3 threatPos, float radius, out Vector3 coverPos)
        {
            //FIXME: Magic numbers
            //Fixme: Разбить, максимальная вложенность 2
            for (int i = 0; i < 10; i++)
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius;

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    if (Physics.Raycast(threatPos + Vector3.up, (hit.position - threatPos).normalized, out RaycastHit rayHit, Vector3.Distance(threatPos, hit.position), _config.ViewMask))
                    {
                        if (rayHit.transform.root != transform.root)
                        {
                            coverPos = hit.position;
                            return true;
                        }
                    }
                }
            }

            coverPos = transform.position;
            return false;
        }
    }
}