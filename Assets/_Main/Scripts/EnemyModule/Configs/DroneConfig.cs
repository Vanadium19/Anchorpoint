using UnityEngine;

namespace EnemyModule
{
    /// <summary>
    /// Designer-facing settings of the kamikaze drone: flight, evasion and explosion.
    /// </summary>
    [CreateAssetMenu(fileName = "DroneConfig", menuName = "Configs/Drone")]
    public class DroneConfig : ScriptableObject
    {
        [Header("Flight")]
        [Tooltip("Cruise speed in meters per second.")]
        [SerializeField] [Min(0.1f)] private float moveSpeed = 7f;
        [Tooltip("Hover height above the ground.")]
        [SerializeField] [Min(0f)] private float flightHeight = 3f;
        [Tooltip("Vertical swaying of the hover, in meters up and down.")]
        [SerializeField] [Min(0f)] private float hoverAmplitude = 1f;
        [Tooltip("Seconds for one full sway of the hover.")]
        [SerializeField] [Min(0.1f)] private float hoverPeriod = 3f;
        [SerializeField] [Min(0f)] private float turnSpeed = 360f;
        [Tooltip("How fast the drone speeds up, in meters per second squared.")]
        [SerializeField] [Min(0.1f)] private float acceleration = 14f;
        [Tooltip("How fast the drone slows down, in meters per second squared.")]
        [SerializeField] [Min(0.1f)] private float deceleration = 20f;

        [Header("Obstacle Avoidance")]
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField] [Min(0f)] private float obstacleCheckDistance = 3f;
        [SerializeField] [Min(0f)] private float obstacleCheckRadius = 0.4f;
        [Tooltip("How strongly the drone is pushed away from the surface it is about to hit.")]
        [SerializeField] [Min(0f)] private float avoidanceStrength = 1f;

        [Header("Evasion")]
        [Tooltip("The drone reacts to a shot whose line passes within this distance of it.")]
        [SerializeField] [Min(0f)] private float evadeTriggerRadius = 1.5f;
        [Tooltip("Chance in percent that the drone dodges a shot aimed at it.")]
        [SerializeField] [Range(0f, 100f)] private float evadeChancePercent = 40f;
        [SerializeField] [Min(0f)] private float evadeDistance = 4f;
        [SerializeField] [Min(0.01f)] private float evadeDuration = 0.3f;
        [Tooltip("Seconds after a dodge during which the drone cannot dodge again.")]
        [SerializeField] [Min(0f)] private float evadeCooldown = 3f;

        [Header("Explosion")]
        [Tooltip("Distance to the player at which the fuse is lit.")]
        [SerializeField] [Min(0f)] private float fuseTriggerDistance = 2.5f;
        [SerializeField] [Min(0f)] private float fuseDelay = 0.5f;
        [SerializeField] [Min(0.01f)] private float fuseBlinkInterval = 0.1f;
        [SerializeField] [Min(0f)] private float explosionDamage = 40f;
        [SerializeField] [Min(0f)] private float explosionRadius = 4f;
        [Tooltip("Radius of the explosion when the drone is shot down before it detonates.")]
        [SerializeField] [Min(0f)] private float shotDownExplosionRadius = 2f;

        public float MoveSpeed => moveSpeed;
        public float FlightHeight => flightHeight;
        public float HoverAmplitude => hoverAmplitude;
        public float HoverPeriod => hoverPeriod;
        public float TurnSpeed => turnSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public LayerMask ObstacleMask => obstacleMask;
        public float ObstacleCheckDistance => obstacleCheckDistance;
        public float ObstacleCheckRadius => obstacleCheckRadius;
        public float AvoidanceStrength => avoidanceStrength;
        public float EvadeTriggerRadius => evadeTriggerRadius;
        public float EvadeChancePercent => evadeChancePercent;
        public float EvadeDistance => evadeDistance;
        public float EvadeDuration => evadeDuration;
        public float EvadeCooldown => evadeCooldown;
        public float FuseTriggerDistance => fuseTriggerDistance;
        public float FuseDelay => fuseDelay;
        public float FuseBlinkInterval => fuseBlinkInterval;
        public float ExplosionDamage => explosionDamage;
        public float ExplosionRadius => explosionRadius;
        public float ShotDownExplosionRadius => shotDownExplosionRadius;
    }
}
