using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Zenject installer that wires up the random events module for one scene.</summary>
    /// <remarks>Everything that varies per scene lives in the scope asset; the installer only points at it.</remarks>
    public class RandomEventsInstaller : MonoInstaller
    {
        [SerializeField] private RandomEventScope scope;
        [SerializeField] private RandomEventNotificationView notificationView;

        /// <summary>Binds the module services, keyed to this scene scope and notification view.</summary>
        public override void InstallBindings()
        {
            Container.BindInstance(scope).AsSingle();
            Container.BindInstance(notificationView).AsSingle();

            Container.BindInterfacesTo<RandomEventStateStore>().AsSingle();
            Container.BindInterfacesTo<RandomEventNotifier>().AsSingle();
            Container.BindInterfacesTo<RandomEventClock>().AsSingle().NonLazy();
            Container.BindInterfacesTo<RandomEventService>().AsSingle();
            Container.BindInterfacesTo<RandomEventTriggerRunner>().AsSingle().NonLazy();

            Container.Bind<RandomEventsSaveable>().AsSingle();
            Container.BindInterfacesTo<RandomEventsSaveHandler>().AsSingle().NonLazy();
            Container.BindExecutionOrder<RandomEventsSaveHandler>(-100);

            Container.BindInterfacesTo<RandomEventNotificationPresenter>().AsSingle().NonLazy();
        }
    }
}
