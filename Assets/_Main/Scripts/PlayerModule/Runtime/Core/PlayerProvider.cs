using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public class PlayerProvider : MonoBehaviour
    {
        [SerializeField] private GameObjectContext context;

        private DiContainer _container;

        private void Awake()
        {
            _container = context.Container;
        }

        private void OnValidate()
        {
            context ??= GetComponent<GameObjectContext>();
        }

        public T Get<T>() where T : class => _container.Resolve<T>();

        public bool TryGet<T>(out T value) where T : class
        {
            value = _container.TryResolve<T>();
            return value != null;
        }
    }
}