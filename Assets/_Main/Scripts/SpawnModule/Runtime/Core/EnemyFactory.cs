using EnemyModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly DiContainer _container;
        private readonly GameObject _prefab;

        public EnemyFactory(DiContainer container, GameObject prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public GameObject Create(Vector3 position)
        {
            return _container.InstantiatePrefab(_prefab, position, Quaternion.identity, null);
        }
    }
}