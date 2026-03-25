using UnityEngine;
using Zenject;

namespace VFXModule
{
    public class VfxServiceInstaller : MonoInstaller
    {
        [SerializeField] private EffectsCatalog effectsCatalog;
        [SerializeField] private Transform effectsContainer;

        public override void InstallBindings()
        {
            Container.Bind<EffectsCatalog>()
                .FromInstance(effectsCatalog)
                .AsSingle();

            Container.BindInterfacesTo<EffectsService>()
                .AsSingle()
                .WithArguments(effectsContainer)
                .NonLazy();
        }
    }
}