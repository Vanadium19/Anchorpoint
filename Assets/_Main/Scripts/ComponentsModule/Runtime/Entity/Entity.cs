using UnityEngine;
using Zenject;

namespace ComponentsModule
{
    public class Entity : MonoBehaviour, IEntity
    {
        [SerializeField] private GameObjectContext context;

        private DiContainer _container;

        private DiContainer Container => _container ??= context.Container;

        private void OnValidate() => context ??= GetComponent<GameObjectContext>();

        public T Get<T>() where T : class => Container.Resolve<T>();

        public bool TryGet<T>(out T value) where T : class
        {
            value = Container.TryResolve<T>();
            return value != null;
        }
    }
}
