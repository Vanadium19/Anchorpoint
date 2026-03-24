using Zenject;

namespace UtilsModule
{
    public static class DIContainerExtensions
    {
        public static T CreateBindAndReturn<T>(this DiContainer container)
        {
            var service = container.Instantiate<T>();
            container.Bind<T>().FromInstance(service).AsSingle();
            return service;
        }
    }
}