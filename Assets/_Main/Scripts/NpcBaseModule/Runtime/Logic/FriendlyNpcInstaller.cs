using UnityEngine;
using Zenject;

namespace NpcModule.Runtime
{
    public sealed class FriendlyNpcInstaller : MonoInstaller
    {
        [SerializeField] private FriendlyNpcView view;

        public override void InstallBindings()
        {
            if (view == null)
                view = GetComponent<FriendlyNpcView>();

            Container.Bind<FriendlyNpcView>().FromInstance(view).AsSingle();

            Container.BindInterfacesAndSelfTo<FriendlyNpcController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
