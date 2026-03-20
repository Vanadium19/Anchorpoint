using UnityEngine;

namespace EnemyModule
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Stats")]
        public float MaxHealth = 100f;

        [Header("Navigation")]
        public float StoppingDistance = 0.5f;
        public float LostTargetReachDistance = 2f;
        public float CoverSearchRadius = 15f;
        public float CoverArrivalDistance = 1f;

        [Header("Sensing")]
        public float SightDistance = 25f;
        public float ViewAngle = 120f;
        public LayerMask ViewMask;
        public float MemoryTime = 10f;

        [Header("Combat")]
        public float AttackRange = 10f;
        public float AttackExitRangeMultiplier = 1.2f;
        public float CombatTurnSpeed = 540f;
        public float FireRate = 1f;
        public int MaxAmmo = 10;
        public float ReloadTime = 3f;
        public float BulletSpeed = 30f;
        public float Damage = 10f;

        [Header("Patrol")]
        public float PatrolWaitTime = 3f;
        public float LookInterval = 2f;
        public float LookTurnSpeed = 2f;
        public float LookAngleRange = 60f;
    }
}