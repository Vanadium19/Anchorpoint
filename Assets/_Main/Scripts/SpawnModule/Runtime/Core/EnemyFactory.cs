using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly DiContainer _container;
        private readonly GameObject _prefab;
        private readonly EnemyTargetRegistry _targetRegistry;
        private readonly EnemyVariants _variants;

        public EnemyFactory(
            DiContainer container,
            GameObject prefab,
            EnemyTargetRegistry targetRegistry,
            [InjectOptional] EnemyVariants variants)
        {
            _container = container;
            _prefab = prefab;
            _targetRegistry = targetRegistry;
            _variants = variants;
        }

        public GameObject Create(Vector3 position)
        {
            if (_variants != null && _variants.TryPick(out var variantPrefab))
                return Create(variantPrefab, position);

            return Create(_prefab, position);
        }

        public GameObject Create(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return null;

            var enemyObject = _container.InstantiatePrefab(
                prefab,
                position,
                Quaternion.identity,
                null);

            _targetRegistry.Register(enemyObject);

            return enemyObject;
        }
    }
}
