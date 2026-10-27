using UnityEngine;

/// <summary>
/// Controls a projectile that follows a target and creates an explosion effect on impact
/// </summary>
namespace Mythmatic.TurretSystem
{
    public class HomingProjectile : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The target that this projectile will follow")]
        public GameObject target;

        [Header("Movement Settings")]
        [Tooltip("How fast the projectile moves in units per second")]
        [Range(1f, 100f)]
        public float speed = 20f;

        [Tooltip("How quickly the projectile can turn in degrees per second")]
        [Range(0f, 720f)]
        public float rotationSpeed = 360f;

        [Tooltip("How aggressively the projectile tracks the target (0-1). Higher values mean tighter tracking")]
        [Range(0f, 10f)]
        public float homingStrength = 10f;

        [Header("Lifetime Settings")]
        [Tooltip("Maximum time in seconds before the projectile self-destructs")]
        [Range(0.1f, 10f)]
        public float maxLifetime = 3f;

        [Header("Effects")]
        [Tooltip("Particle effect prefab spawned when projectile hits target")]
        [SerializeField] private GameObject _explosionPrefab;
        public GameObject explosionPrefab
        {
            get { return _explosionPrefab; }
            set { _explosionPrefab = value; }
        }

        [Tooltip("Number of particles to emit on impact")]
        [Range(1, 100)]
        public int particleCount = 30;

        // Private runtime variables
        private Rigidbody rb;
        private float creationTime;
        private float initialSpeed;

        private void Awake()
        {
            // Store initial speed set in inspector
            initialSpeed = speed;
        }

        /// <summary>
        /// Sets up the projectile with its target and optional speed override
        /// </summary>
        public void Initialize(GameObject enemy, float? projectileSpeed = null)
        {
            target = enemy;
            // Only override speed if a new value is provided
            if (projectileSpeed.HasValue)
            {
                speed = projectileSpeed.Value;
            }

            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            creationTime = Time.time;

            // Set initial velocity
            if (target != null)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                rb.linearVelocity = direction * speed;
            }
        }

        private void FixedUpdate()
        {
            if (target == null || rb == null)
            {
                DestroyProjectile();
                return;
            }

            if (Time.time - creationTime > maxLifetime)
            {
                DestroyProjectile();
                return;
            }

            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            transform.rotation = rotation;

            Vector3 newVelocity = Vector3.Lerp(
                rb.linearVelocity.normalized,
                directionToTarget,
                homingStrength * Time.fixedDeltaTime
            ).normalized * speed;

            rb.linearVelocity = newVelocity;
        }

        private void SpawnExplosion(Vector3 position)
        {
            if (_explosionPrefab != null)
            {
                // Instantiate the explosion prefab
                GameObject explosion = Instantiate(_explosionPrefab, position, Quaternion.identity);

                // Get the particle system component
                ParticleSystem particleSystem = explosion.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    // Stop default emission
                    var emission = particleSystem.emission;
                    emission.enabled = false;

                    // Emit particles immediately
                    particleSystem.Emit(particleCount);
                }

                // Clean up the explosion object after particles have played
                float lifetime = particleSystem != null ? particleSystem.main.duration : 1f;
                Destroy(explosion, lifetime);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject == target)
            {
                // Use the first contact point for the explosion position
                Vector3 impactPosition = collision.contacts[0].point;
                SpawnExplosion(impactPosition);
            }
            DestroyProjectile();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == target)
            {
                // For trigger, use the current position
                SpawnExplosion(transform.position);
            }
            DestroyProjectile();
        }

        private void DestroyProjectile()
        {
            Destroy(gameObject);
        }
    }
}