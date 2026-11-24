using UnityEngine;
using Zenject;

namespace EnemyModule
{
    /// <summary>
    /// Gives an enemy the ability to throw grenades. Used next to <see cref="EnemyInstaller"/> on the grenadier prefab.
    /// </summary>
    public class GrenadierInstaller : MonoInstaller
    {
        [SerializeField] private GrenadierConfig config;
        [SerializeField] private Transform throwPoint;
        [SerializeField] private GameObject grenadePrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();

            Container.Bind<IExplosionDamageService>()
                .To<ExplosionDamageService>()
                .AsSingle();

            Container.Bind<IGrenadeThrowComponent>()
                .To<GrenadeThrowComponent>()
                .AsSingle()
                .WithArguments(throwPoint, grenadePrefab);
        }
    }
}
