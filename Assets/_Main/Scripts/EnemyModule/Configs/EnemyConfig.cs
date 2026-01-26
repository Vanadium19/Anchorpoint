using UnityEngine;

namespace EnemyModule.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;

        [Header("Sensing")]
        public float SightDistance = 25f;
        public float ViewAngle = 120f;
        public LayerMask ViewMask;
        public float MemoryTime = 10f;

        [Header("Combat")]
        public float AttackRange = 10f;
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
        public float MaxHealth => maxHealth;
    }
}