using InventoryModule;
using UnityEngine;
using UtilsModule;

namespace TradeModule
{
    /// <summary>
    /// World component that marks a scene object as a trader and exposes its offers.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IExternalUI"/> so <c>PlayerModule.PlayerInteractionController</c> opens
    /// the assigned <see cref="UIPrefab"/> through <c>InventoryModule.ExternalUIManager</c> without any
    /// change to player interaction code. This component carries no health or damage component, so a
    /// trader cannot be damaged or killed. It holds no trading logic; that lives in
    /// <see cref="TradeService"/> and <see cref="TradePresenter"/>.
    /// </remarks>
    public class TraderView : MonoBehaviour, IExternalUI, ITraderOffersProvider
    {
        [SerializeField] private string displayName = "Trader";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private TraderOffersConfig offers;

        /// <inheritdoc/>
        public string DisplayName =>
            offers != null && !string.IsNullOrEmpty(offers.TraderNameKey)
                ? LocalizedText.Get(offers.TraderNameKey)
                : displayName;

        /// <inheritdoc/>
        public GameObject UIPrefab => uiPrefab;

        /// <inheritdoc/>
        public TraderOffersConfig Offers => offers;
    }
}

