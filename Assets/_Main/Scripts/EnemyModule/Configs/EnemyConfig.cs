using System.Collections.Generic;
using System.Linq;
using InventoryModule;
using UnityEngine;
using Random = UnityEngine.Random;

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

        [Header("Structures")]
        public float StructureAttackRange = 4f;
        public float StructureDamage = 10f;
        public float StructureAttackRate = 1f;

        [Header("Patrol")]
        public float PatrolWaitTime = 3f;
        public float LookInterval = 2f;
        public float LookTurnSpeed = 2f;
        public float LookAngleRange = 60f;

        [Header("Loot")]
        [Min(0)] [SerializeField] private int maxDropItems;
        [SerializeField] private List<LootDropData> loot;
        [SerializeField] private float lootScatterRadius = 0.75f;
        [SerializeField] private float lootSpawnOffsetY = 0.35f;

        public float LootScatterRadius => lootScatterRadius;
        public float LootSpawnOffsetY => lootSpawnOffsetY;

        public int GetRandomDropCount() => maxDropItems <= 0 ? 0 : Random.Range(0, maxDropItems + 1);

        public bool TryGetRandomLootItem(out ItemDataSo item)
        {
            item = null;

            if (loot == null || loot.Count == 0)
                return false;

            var totalWeight = loot.Sum(entry => entry.DropWeight);

            if (totalWeight <= 0f)
                return false;

            var roll = Random.value * totalWeight;
            var fallbackItem = default(ItemDataSo);

            foreach (var data in loot)
            {
                if (!data.IsValid)
                    continue;

                fallbackItem ??= data.Item;
                roll -= data.DropWeight;

                if (roll > 0f)
                    continue;

                item = data.Item;
                return true;
            }

            item = fallbackItem;
            return item;
        }
    }
}