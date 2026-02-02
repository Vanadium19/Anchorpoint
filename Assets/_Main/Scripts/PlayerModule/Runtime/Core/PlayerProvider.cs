using UnityEngine;
using Zenject;
using ComponentsModule;

namespace PlayerModule
{
    public class PlayerProvider : MonoBehaviour, IPlayerPositionProvider
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
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public Vector3 Forward => transform.forward;
    }
}