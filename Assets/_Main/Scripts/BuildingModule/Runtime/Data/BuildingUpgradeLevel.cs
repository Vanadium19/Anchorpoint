using System;
using System.Collections.Generic;
using BaseModule;
using UnityEngine;

namespace BuildingModule
{
    [Serializable]
    public class BuildingUpgradeLevel
    {
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Price price;
        [SerializeField] [Min(0f)] private float maxHealth;

        [SerializeReference] private List<BuildingUpgradeEffect> effects = new();

        public GameObject VisualPrefab => visualPrefab;
        public Price Price => price;
        public float MaxHealth => maxHealth;
        public IReadOnlyList<BuildingUpgradeEffect> Effects => effects;
    }
}
