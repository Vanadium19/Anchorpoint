using UnityEngine;
using Zenject;

namespace NpcBaseModule
{
    public sealed class FriendlyNpcInstaller : MonoInstaller
    {
        [SerializeField] private FriendlyNpcView view;

        private void OnValidate() => view ??= GetComponent<FriendlyNpcView>();

        public override void InstallBindings()
        {
            Container.Bind<FriendlyNpcView>()
                .FromInstance(view)
                .AsSingle();

            Container.BindInterfacesAndSelfTo<FriendlyNpcController>()
                .AsSingle()
                .NonLazy();
        }
    }
}