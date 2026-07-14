using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public abstract class ExternalUIHandler : IInitializable, IDisposable
    {
        private readonly Dictionary<IExternalUI, GameObject> _cache = new();
        private readonly ExternalUIManager _manager;
        protected readonly DiContainer DiContainer;
        private readonly Canvas _canvas;

        protected ExternalUIHandler(ExternalUIManager manager, DiContainer diContainer, Canvas canvas)
        {
            _manager = manager;
            DiContainer = diContainer;
            _canvas = canvas;
        }

        public void Initialize()
        {
            _manager.UIOpened += OnUIOpened;
            _manager.UIClosed += CloseAll;
        }

        public void Dispose()
        {
            _manager.UIOpened -= OnUIOpened;
            _manager.UIClosed -= CloseAll;
        }

        private void OnUIOpened(IExternalUI ui)
        {
            if (!CanHandle(ui))
                return;

            if (_cache.TryGetValue(ui, out var cached))
            {
                cached.SetActive(true);
                OnReactivated(ui, cached);
            }
            else
            {
                var view = CreateView(ui);
                _cache[ui] = view;
                OnCreated(ui, view);
            }
        }

        public virtual void CloseAll()
        {
            foreach (var go in _cache.Values)
            {
                if (go != null)
                    go.SetActive(false);
            }
        }

        public void Close(IExternalUI ui)
        {
            if (_cache.TryGetValue(ui, out var go) && go != null)
                go.SetActive(false);
        }

        protected GameObject InstantiateView(IExternalUI ui)
        {
            return DiContainer.InstantiatePrefab(ui.UIPrefab, _canvas.transform);
        }

        protected abstract bool CanHandle(IExternalUI ui);
        protected abstract GameObject CreateView(IExternalUI ui);
        protected virtual void OnReactivated(IExternalUI ui, GameObject view) { }
        protected virtual void OnCreated(IExternalUI ui, GameObject view) { }
    }
}