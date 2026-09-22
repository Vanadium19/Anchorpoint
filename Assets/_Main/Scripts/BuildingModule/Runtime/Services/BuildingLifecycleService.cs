using System;
using System.Collections.Generic;
using BaseModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingLifecycleService : IInitializable, ITickable, IDisposable, IPausable
    {
        private readonly List<BuildingController> _controllers = new();
        private readonly IPauseManager _pauseManager;

        private bool _isPaused;

        public BuildingLifecycleService(IPauseManager pauseManager)
        {
            _pauseManager = pauseManager;
        }

        public void Initialize() => _pauseManager.Register(this);

        public void Tick()
        {
            if (_isPaused)
                return;

            for (var i = _controllers.Count - 1; i >= 0; i--)
            {
                var controller = _controllers[i];

                if (controller.IsDestroyed)
                {
                    controller.Dispose();
                    _controllers.RemoveAt(i);
                    continue;
                }

                controller.Tick(Time.deltaTime);
            }
        }

        public void Dispose()
        {
            _pauseManager.Unregister(this);

            foreach (var controller in _controllers)
                controller.Dispose();

            _controllers.Clear();
        }

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        public void Register(BuildingController controller)
        {
            if (controller == null || _controllers.Contains(controller))
                return;

            _controllers.Add(controller);
        }
    }
}
