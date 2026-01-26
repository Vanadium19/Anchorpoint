using UnityEngine;
using Zenject;

namespace NpcModule.Runtime
{
    public sealed class FriendlyNpcInstaller : MonoInstaller
    {
        [SerializeField] private FriendlyNpcView view;

        private void OnValidate()
        {
            view ??= GetComponent<FriendlyNpcView>();
        }

        private void Reset()
        {
            view ??= GetComponent<FriendlyNpcView>();
        }

        public override void InstallBindings()
        {
            view ??= GetComponent<FriendlyNpcView>();

            Container.Bind<FriendlyNpcView>()
                .FromInstance(view)
                .AsSingle();

            Container.BindInterfacesAndSelfTo<FriendlyNpcController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
