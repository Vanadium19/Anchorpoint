using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public interface IInventoryItemPool
    {
        InventoryItemView Get();
        void Return(InventoryItemView itemView);
        void WarmUp(int count);
    }

    public sealed class InventoryItemPool : IInventoryItemPool
    {
        private readonly InventoryItemView _prefab;
        private readonly Transform _container;
        private readonly Queue<InventoryItemView> _pool;

        public InventoryItemPool(InventoryItemView prefab, Transform container)
        {
            _prefab = prefab;
            _container = container;
            _pool = new Queue<InventoryItemView>();
        }

        public InventoryItemView Get()
        {
            if (_pool.Count > 0)
            {
                var itemView = _pool.Dequeue();
                itemView.ResetView();
                itemView.gameObject.SetActive(true);
                return itemView;
            }

            var newItem = GameObject.Instantiate(_prefab, _container);
            newItem.ResetView();
            return newItem;
        }

        public void Return(InventoryItemView itemView)
        {
            if (itemView == null)
                return;

            itemView.ResetView();
            itemView.gameObject.SetActive(false);
            _pool.Enqueue(itemView);
        }

        public void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var itemView = Get();
                itemView.gameObject.SetActive(false);
                _pool.Enqueue(itemView);
            }
        }
    }
}
