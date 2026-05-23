using System.Collections.Generic;
using InventoryModule;
using UnityEngine;

namespace BaseModule
{
    public class WorkbenchQueueView : MonoBehaviour
    {
        [SerializeField] private Transform queueContainer;
        [SerializeField] private WorkbenchQueueEntry queueEntryPrefab;
        [SerializeField] private Transform readyContainer;
        [SerializeField] private WorkbenchQueueEntry readyEntryPrefab;

        private ICraftService _craftService;
        private IInventoryManager _inventoryManager;
        private readonly List<WorkbenchQueueEntry> _activeEntries = new();
        private readonly List<WorkbenchQueueEntry> _readyEntries = new();

        public void Initialize(ICraftService craftService, IInventoryManager inventoryManager)
        {
            _craftService = craftService;
            _inventoryManager = inventoryManager;
        }

        public void Refresh()
        {
            ClearEntries(_activeEntries);
            ClearEntries(_readyEntries);

            foreach (var batch in _craftService.ActiveBatches)
            {
                var entry = Instantiate(queueEntryPrefab, queueContainer);
                entry.SetupBatch(batch, _craftService);
                _activeEntries.Add(entry);
            }

            foreach (var ready in _craftService.ReadyItems)
            {
                var entry = Instantiate(readyEntryPrefab, readyContainer);
                entry.SetupReady(ready, _craftService, _inventoryManager);
                _readyEntries.Add(entry);
            }
        }

        private void ClearEntries(List<WorkbenchQueueEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry != null)
                    Destroy(entry.gameObject);
            }

            entries.Clear();
        }
    }
}