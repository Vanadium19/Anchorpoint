using Zenject;

namespace ArsenalModule
{
    /// <summary>
    /// Registers the arsenal module's bindings with Zenject.
    /// </summary>
    public class ArsenalInstaller : MonoInstaller
    {
        /// <inheritdoc/>
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ArsenalService>().AsSingle();
        }
    }
}
