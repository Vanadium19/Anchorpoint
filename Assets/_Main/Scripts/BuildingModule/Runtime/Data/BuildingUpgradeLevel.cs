using System;
using UnityEngine;

namespace BuildingModule
{
    [Serializable]
    public class BuildingUpgradeLevel
    {
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Price price;
        [SerializeField] [Min(0f)] private float maxHealth;

        public GameObject VisualPrefab => visualPrefab;
        public Price Price => price;
        public float MaxHealth => maxHealth;
    }
}
