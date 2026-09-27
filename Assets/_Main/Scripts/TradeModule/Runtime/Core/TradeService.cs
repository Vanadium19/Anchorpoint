using System;
using System.Collections.Generic;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace TradeModule
{
    /// <summary>
    /// Handles trader UI screens and executes trades against the player's inventory.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>BaseModule.CraftService</c>: it is the <see cref="ExternalUIHandler"/> for every world
    /// object that exposes <see cref="ITraderOffersProvider"/>, and it owns the single source of truth
    /// for whether a trade can be completed.
    /// </remarks>
    public class TradeService : ExternalUIHandler, ITradeService
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly Dictionary<IExternalUI, TradePresenter> _presenters = new();

        /// <inheritdoc/>
        public event Action TradeStateChanged;

        /// <summary>
        /// Creates the service with the inventory it trades against and the dependencies required to
        /// instantiate trader UI screens.
        /// </summary>
        public TradeService(
            IInventoryManager inventoryManager,
            ExternalUIManager manager,
            DiContainer diContainer,
            Canvas canvas)
            : base(manager, diContainer, canvas)
        {
            _inventoryManager = inventoryManager;
        }

        /// <inheritdoc/>
        public bool CanTrade(TradeOfferConfig offer)
        {
            if (offer == null)
                return false;

            foreach (var request in offer.Requested)
            {
                if (_inventoryManager.GetItemCount(request.Item) < request.Count)
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool TryTrade(TradeOfferConfig offer)
        {
            if (!CanTrade(offer))
                return false;

            foreach (var request in offer.Requested)
                _inventoryManager.TryRemoveItems(request.Item, request.Count);

            foreach (var reward in offer.Offered)
                _inventoryManager.AddItemToInventory(reward.Item, reward.Count);

            TradeStateChanged?.Invoke();

            return true;
        }

        /// <inheritdoc/>
        protected override bool CanHandle(IExternalUI ui) => ui is ITraderOffersProvider;

        /// <inheritdoc/>
        protected override GameObject CreateView(IExternalUI ui)
        {
            var instance = InstantiateView(ui);
            var view = instance.GetComponent<TradeView>();

            if (view != null && ui is ITraderOffersProvider provider)
            {
                var presenter = new TradePresenter(this, view, provider.Offers);
                presenter.Initialize();
                _presenters[ui] = presenter;
            }

            return instance;
        }

        /// <inheritdoc/>
        protected override void OnReactivated(IExternalUI ui, GameObject view)
        {
            if (_presenters.TryGetValue(ui, out var presenter))
                presenter.Refresh();
        }
    }
}
