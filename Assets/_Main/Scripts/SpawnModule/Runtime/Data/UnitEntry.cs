using System;
using UnityEngine;

namespace SpawnModule
{
    [Serializable]
    public class UnitEntry
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int weight = 1;

        public GameObject Prefab => prefab;
        public int Weight => weight;

#if UNITY_EDITOR
        public void SetPrefab(GameObject newPrefab) => prefab = newPrefab;
        public void SetWeight(int newWeight) => weight = newWeight;
#endif
    }
}
