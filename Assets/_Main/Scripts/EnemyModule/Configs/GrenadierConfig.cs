using UnityEngine;

namespace EnemyModule
{
    /// <summary>
    /// Designer-facing settings of the grenadier: throw rhythm, retreat distance and the grenade itself.
    /// </summary>
    [CreateAssetMenu(fileName = "GrenadierConfig", menuName = "Configs/Grenadier")]
    public class GrenadierConfig : ScriptableObject
    {
        [Header("Throw")]
        [Tooltip("Distance to the player at which the grenadier stops approaching and starts throwing.")]
        [SerializeField] [Min(0f)] private float throwDistance = 15f;
        [Tooltip("When the player gets closer than this, the grenadier steps back to this distance. 0 disables the retreat.")]
        [SerializeField] [Min(0f)] private float retreatDistance = 6f;
        [Tooltip("Seconds between the starts of two throws.")]
        [SerializeField] [Min(0.1f)] private float throwInterval = 4f;
        [Tooltip("Seconds from the start of the throw animation to the moment the grenade leaves the hand.")]
        [SerializeField] [Min(0f)] private float throwWindupDuration = 0.8f;
        [Tooltip("Horizontal speed of the grenade in flight, in meters per second.")]
        [SerializeField] [Min(0.1f)] private float grenadeFlightSpeed = 12f;

        [Header("Grenade")]
        [Tooltip("Seconds from the throw to the explosion.")]
        [SerializeField] [Min(0f)] private float fuseTime = 2.5f;
        [SerializeField] [Min(0f)] private float explosionDamage = 30f;
        [SerializeField] [Min(0f)] private float explosionRadius = 4f;
        [Tooltip("Shows a red circle with the explosion radius at the landing point until the grenade explodes.")]
        [SerializeField] private bool shouldShowDangerMarker = true;
        [Tooltip("Layers the landing point is projected onto.")]
        [SerializeField] private LayerMask groundMask = ~0;

        public float ThrowDistance => throwDistance;
        public float RetreatDistance => retreatDistance;
        public float ThrowInterval => throwInterval;
        public float ThrowWindupDuration => throwWindupDuration;
        public float GrenadeFlightSpeed => grenadeFlightSpeed;
        public float FuseTime => fuseTime;
        public float ExplosionDamage => explosionDamage;
        public float ExplosionRadius => explosionRadius;
        public bool ShouldShowDangerMarker => shouldShowDangerMarker;
        public LayerMask GroundMask => groundMask;
    }
}
