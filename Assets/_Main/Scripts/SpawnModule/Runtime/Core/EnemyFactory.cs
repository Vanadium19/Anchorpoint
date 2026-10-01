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

        public GameObject Create(Vector3 position) => Create(_prefab, position);

        public GameObject Create(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return null;

            return _container.InstantiatePrefab(prefab, position, Quaternion.identity, null);
        }
    }
}
