using System;
using ComponentsModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class ExternalUIManager : IInitializable, ITickable, IDisposable
    {
        private readonly IInventoryManager _inventory;
        private readonly GameObject _externalPanel;
        private readonly GameObject _playerUI;

        public event Action<IExternalUI> UIOpened;
        public event Action UIClosed;

        public ExternalUIManager(IInventoryManager inventory, GameObject externalPanel, GameObject playerUI)
        {
            _inventory = inventory;
            _externalPanel = externalPanel;
            _playerUI = playerUI;
        }

        public IExternalUI Current { get; private set; }

        public void Initialize()
        {
            _inventory.InventoryOpened += OnInventoryOpened;
            _inventory.InventoryClosed += OnInventoryClosed;
        }

        public void Tick()
        {
            if (Current is not Component component)
                return;

            if (!InteractionGateUtility.IsAllowed(component.transform))
                _inventory.CloseInventory();
        }

        public void Dispose()
        {
            _inventory.InventoryOpened -= OnInventoryOpened;
            _inventory.InventoryClosed -= OnInventoryClosed;
        }

        public void Open(IExternalUI ui)
        {
            if (ui is Component component && !InteractionGateUtility.IsAllowed(component.transform))
                return;

            _inventory.OpenInventory();
            Current = ui;
            HideExternalPanel();
            UIOpened?.Invoke(ui);
        }

        private void OnInventoryOpened()
        {
            HidePlayerUI();

            if (Current == null)
                ShowExternalPanel();
        }

        private void OnInventoryClosed()
        {
            Current = null;
            ShowPlayerUI();
            UIClosed?.Invoke();
        }

        private void HideExternalPanel()
        {
            if (_externalPanel != null)
                _externalPanel.SetActive(false);
        }

        private void ShowExternalPanel()
        {
            if (_externalPanel != null && !_externalPanel.activeSelf)
                _externalPanel.SetActive(true);
        }

        private void HidePlayerUI()
        {
            if (_playerUI != null && _playerUI.activeSelf)
                _playerUI.SetActive(false);
        }

        private void ShowPlayerUI()
        {
            if (_playerUI != null && !_playerUI.activeSelf)
                _playerUI.SetActive(true);
        }
    }
}
