using Zenject;

namespace TradeModule
{
    /// <summary>
    /// Registers the trade module's bindings with Zenject.
    /// </summary>
    public class TradeInstaller : MonoInstaller
    {
        /// <inheritdoc/>
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TradeService>().AsSingle();
        }
    }
}
