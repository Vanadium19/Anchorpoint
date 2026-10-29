using Zenject;

namespace ArsenalModule
{
    /// <summary>
    /// Registers the Arsenal module's bindings with Zenject.
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
