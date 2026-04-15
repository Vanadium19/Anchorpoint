using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class DeathLootStorage : IDeathLootStorage
    {
        private readonly List<DeathLootPileData> _piles = new();

        public DeathLootPileData CreatePile(string sceneName, Vector3 position, List<ItemTable> items)
        {
            var pile = new DeathLootPileData(sceneName, position, items);

            if (!pile.HasItems)
                return null;

            _piles.Add(pile);
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

                var removed = pile.RemoveItem(item);

                if (removed && !pile.HasItems)
                    _piles.RemoveAt(i);
            }
        }
    }
}