using Zenject;

namespace WeaponModule.Installers
{
    /// <summary>
    /// Scene-level bindings of weapon modifications, shared by the player's weapons and scene stations.
    /// </summary>
    public class WeaponModificationInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IWeaponModificationService>().To<WeaponModificationService>().AsSingle();
        }
    }
}
