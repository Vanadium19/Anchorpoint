using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Zenject installer that wires up the random events module for one scene.</summary>
    public class RandomEventsInstaller : MonoInstaller
    {
        [SerializeField] private RandomEventsRules rules;
        [SerializeField] private RandomEventScope scope;
        [SerializeField] private RandomEventNotificationView notificationView;

        /// <summary>Binds the module's services, keyed to this scene's rules, scope and notification view.</summary>
        public override void InstallBindings()
        {
            Container.BindInstance(rules).AsSingle();
            Container.BindInstance(scope).AsSingle();
            Container.BindInstance(notificationView).AsSingle();

            Container.BindInterfacesTo<RandomEventStateStore>().AsSingle();
            Container.BindInterfacesTo<RandomEventNotifier>().AsSingle();
            Container.BindInterfacesTo<RandomEventService>().AsSingle();
            Container.BindInterfacesTo<RandomEventTriggerRunner>().AsSingle().NonLazy();

            Container.Bind<RandomEventsSaveable>().AsSingle();
            Container.BindInterfacesTo<RandomEventsSaveHandler>().AsSingle().NonLazy();
            Container.BindExecutionOrder<RandomEventsSaveHandler>(-100);

            Container.BindInterfacesTo<RandomEventNotificationPresenter>().AsSingle().NonLazy();
        }
    }
}
