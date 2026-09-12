using Zenject;

namespace RandomEventsModule
{
    /// <summary>Zenject installer for the relays that turn base facts into random event signals.</summary>
    /// <remarks>
    /// The relays are counted state, not services: a fact reported from two scenes at once is
    /// counted twice. Only the scene that owns the fact installs them, so the shooter scene takes
    /// <see cref="RandomEventsInstaller"/> alone while the base scene takes both.
    /// </remarks>
    public class RandomEventSignalsInstaller : MonoInstaller
    {
        /// <summary>Binds the base signal relays of this scene.</summary>
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<WorkbenchUsageRelay>().AsSingle().NonLazy();
            Container.BindInterfacesTo<RaidChanceRelay>().AsSingle().NonLazy();
        }
    }
}
