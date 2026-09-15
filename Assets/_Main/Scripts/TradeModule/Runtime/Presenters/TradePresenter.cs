using System.Linq;
using UtilsModule;

namespace TradeModule
{
    /// <summary>
    /// Wires a <see cref="TradeView"/> to an <see cref="ITradeService"/> for a single trader.
    /// </summary>
    /// <remarks>
    /// Owns every dependency the view is not allowed to touch: it resolves item and title text through
    /// <see cref="LocalizedText"/>, asks the service whether each offer is affordable, and re-renders
    /// the view whenever <see cref="ITradeService.TradeStateChanged"/> fires.
    /// </remarks>
    public class TradePresenter
    {
        private readonly ITradeService _tradeService;
        private readonly TradeView _view;
        private readonly TraderOffersConfig _offers;

        /// <summary>
        /// Creates the presenter for one trader's view and offers.
        /// </summary>
        /// <param name="tradeService">The service used to check and perform trades.</param>
        /// <param name="view">The view to render into.</param>
        /// <param name="offers">The trader's configured offers.</param>
        public TradePresenter(ITradeService tradeService, TradeView view, TraderOffersConfig offers)
        {
            _tradeService = tradeService;
            _view = view;
            _offers = offers;
        }

        /// <summary>
        /// Subscribes to view and service events and performs the first render.
        /// </summary>
        public void Initialize()
        {
            _view.OfferSelected += OnOfferSelected;
            _view.Closed += Dispose;
            _tradeService.TradeStateChanged += OnTradeStateChanged;

            if (_offers != null && !string.IsNullOrEmpty(_offers.TraderNameKey))
                _view.SetTraderName(LocalizedText.Get(_offers.TraderNameKey));

            Refresh();
        }

        /// <summary>
        /// Unsubscribes from the trade service so the presenter no longer receives updates.
        /// </summary>
        public void Dispose()
        {
            _view.OfferSelected -= OnOfferSelected;
            _view.Closed -= Dispose;
            _tradeService.TradeStateChanged -= OnTradeStateChanged;
        }

        /// <summary>
        /// Rebuilds display data for every offer and re-renders the view.
        /// </summary>
        public void Refresh()
        {
            if (_offers == null)
                return;

            var displayData = _offers.Offers
                .Select(BuildDisplayData)
                .ToList();

            _view.Render(displayData);
        }

        private TradeOfferDisplayData BuildDisplayData(TradeOfferConfig offer)
        {
            return new TradeOfferDisplayData
            {
                Offer = offer,
                TitleText = LocalizedText.Get(offer.TitleKey),
                RequestedItems = offer.Requested,
                OfferedItems = offer.Offered,
                CanAfford = _tradeService.CanTrade(offer)
            };
        }

        private void OnOfferSelected(TradeOfferConfig offer) => _tradeService.TryTrade(offer);

        private void OnTradeStateChanged() => Refresh();
    }
}
