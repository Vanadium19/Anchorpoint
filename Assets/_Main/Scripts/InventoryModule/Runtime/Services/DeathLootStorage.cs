using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class DeathLootStorage : IDeathLootStorage
    {
        private readonly List<DeathLootPileData> _piles = new();

        internal event Action PilesChanged;

        internal IReadOnlyList<DeathLootPileData> Piles => _piles;

        public DeathLootPileData CreatePile(string sceneName, Vector3 position, List<ItemTable> items)
        {
            var pile = new DeathLootPileData(sceneName, position, items);

            if (!pile.HasItems)
                return null;

            _piles.Add(pile);
            PilesChanged?.Invoke();

            return pile;
        }

        public IReadOnlyList<DeathLootPileData> GetPiles(string sceneName)
        {
            var result = new List<DeathLootPileData>();

            for (var i = 0; i < _piles.Count; i++)
            {
                var pile = _piles[i];

                if (pile == null || !pile.HasItems)
                    continue;

                if (pile.SceneName == sceneName)
                    result.Add(pile);
            }

            return result;
        }

        public void RemoveItem(ItemTable item)
        {
            if (item == null)
                return;

            for (var i = _piles.Count - 1; i >= 0; i--)
            {
                var pile = _piles[i];

                if (pile == null)
                    continue;

                if (!pile.RemoveItem(item))
                    continue;

                if (!pile.HasItems)
                    _piles.RemoveAt(i);

                PilesChanged?.Invoke();
                return;
            }
        }

        internal void ReplacePiles(IReadOnlyList<DeathLootPileData> piles)
        {
            _piles.Clear();

            if (piles == null)
                return;

            for (var i = 0; i < piles.Count; i++)
            {
                var pile = piles[i];

                if (pile != null && pile.HasItems)
                    _piles.Add(pile);
            }
        }
    }
}
