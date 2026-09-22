using System;
using UnityEngine;

namespace BuildingModule
{
    [Serializable]
    public class BuildingUpgradeLevel
    {
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Price price;

        public GameObject VisualPrefab => visualPrefab;
        public Price Price => price;
    }
}
