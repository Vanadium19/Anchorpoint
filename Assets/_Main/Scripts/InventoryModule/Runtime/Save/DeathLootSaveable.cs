using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SaveModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class DeathLootSaveable : IInitializable, IDisposable, ISaveable
    {
        private const string DeathLootKey = "death_loot";

        private readonly DeathLootStorage _deathLootStorage;
        private readonly ItemCatalog _itemCatalog;
        private readonly IGameSaveLoader _gameSaveLoader;

        public DeathLootSaveable(
            DeathLootStorage deathLootStorage,
            ItemCatalog itemCatalog,
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader)
        {
            _deathLootStorage = deathLootStorage;
            _itemCatalog = itemCatalog;
            _gameSaveLoader = gameSaveLoader;
        }

        public string SaveKey => DeathLootKey;

        public void Initialize()
        {
            _deathLootStorage.PilesChanged += OnPilesChanged;
        }

        public void Dispose()
        {
            _deathLootStorage.PilesChanged -= OnPilesChanged;
        }

        public string CreateMemento()
        {
            var storageMemento = new DeathLootStorageMemento();
            var piles = _deathLootStorage.Piles;

            for (var i = 0; i < piles.Count; i++)
            {
                var pileMemento = SerializePile(piles[i]);

                if (pileMemento != null)
                    storageMemento.Piles.Add(pileMemento);
            }

            return JsonConvert.SerializeObject(storageMemento);
        }

        public void RestoreMemento(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                _deathLootStorage.ReplacePiles(Array.Empty<DeathLootPileData>());
                return;
            }

            var storageMemento = JsonConvert.DeserializeObject<DeathLootStorageMemento>(data);
            var piles = new List<DeathLootPileData>();

            if (storageMemento?.Piles != null)
            {
                for (var i = 0; i < storageMemento.Piles.Count; i++)
                {
                    var pile = DeserializePile(storageMemento.Piles[i]);

                    if (pile != null)
                        piles.Add(pile);
                }
            }

            _deathLootStorage.ReplacePiles(piles);
        }

        private DeathLootPileMemento SerializePile(DeathLootPileData pile)
        {
            if (pile == null || !pile.HasItems)
                return null;

            var pileMemento = new DeathLootPileMemento
            {
                SceneName = pile.SceneName,
                PositionX = pile.Position.x,
                PositionY = pile.Position.y,
                PositionZ = pile.Position.z
            };

            var items = pile.Items;

            for (var i = 0; i < items.Count; i++)
            {
                var itemMemento = ItemSerializer.Serialize(items[i]);

                if (itemMemento != null)
                    pileMemento.Items.Add(itemMemento);
            }

            return pileMemento.Items.Count > 0 ? pileMemento : null;
        }

        private DeathLootPileData DeserializePile(DeathLootPileMemento pileMemento)
        {
            if (pileMemento == null || string.IsNullOrEmpty(pileMemento.SceneName))
                return null;

            var items = new List<ItemTable>();

            if (pileMemento.Items != null)
            {
                for (var i = 0; i < pileMemento.Items.Count; i++)
                {
                    var item = DeserializeItem(pileMemento.Items[i]);

                    if (item != null)
                        items.Add(item);
                }
            }

            var position = new Vector3(
                pileMemento.PositionX,
                pileMemento.PositionY,
                pileMemento.PositionZ);

            var pile = new DeathLootPileData(pileMemento.SceneName, position, items);
            return pile.HasItems ? pile : null;
        }

        private ItemTable DeserializeItem(ItemMemento itemMemento)
        {
            if (itemMemento == null || string.IsNullOrEmpty(itemMemento.ItemDataName))
                return null;

            var itemData = _itemCatalog.GetByName(itemMemento.ItemDataName);

            if (itemData == null)
                return null;

            var item = ItemSerializer.Deserialize(itemMemento, itemData);

            if (item == null)
                return null;

            ItemSerializer.RestoreNestedContainers(item, itemMemento, _itemCatalog);
            return item;
        }

        private void OnPilesChanged()
        {
            _gameSaveLoader.Save();
        }
    }
}
