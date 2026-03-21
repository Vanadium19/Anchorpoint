using System;
using System.Collections.Generic;
using BuildingModule;
using InputModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public class PlayerInputSwitcher : IInitializable, IDisposable
    {
        private readonly IInputMap _inputMap;
        private readonly IConstructionModeService _constructionModeService;

        private readonly Dictionary<PlayerInputMode, IPlayerInput> _inputs;
        private PlayerInputMode _currentMode;
        private IPlayerInput _currentInput;
        private bool _isInitialized;

        public IPlayerInput CurrentInput
        {
            get
            {
                EnsureInitialized();
                return _currentInput;
            }
        }

        public PlayerInputMode CurrentMode => _currentMode;

        public PlayerInputSwitcher(
            IInputMap inputMap,
            [InjectOptional] IConstructionModeService constructionModeService = null)
        {
            _inputMap = inputMap;
            _constructionModeService = constructionModeService;

            _inputs = new Dictionary<PlayerInputMode, IPlayerInput>
            {
                { PlayerInputMode.Normal, new NormalPlayerInput(inputMap) },
                { PlayerInputMode.Build, new BuildModePlayerInput(inputMap) }
            };
        }

        public void Initialize()
        {
            EnsureInitialized();

            if (_constructionModeService != null)
                _constructionModeService.ActiveChanged += OnConstructionModeChanged;
        }

        public void Dispose()
        {
            if (_constructionModeService != null)
                _constructionModeService.ActiveChanged -= OnConstructionModeChanged;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _currentMode = PlayerInputMode.Normal;
            _currentInput = _inputs[PlayerInputMode.Normal];
            _isInitialized = true;
        }

        private void OnConstructionModeChanged(bool isActive)
        {
            var newMode = isActive ? PlayerInputMode.Build : PlayerInputMode.Normal;
            SwitchMode(newMode);
        }

        public void SwitchMode(PlayerInputMode mode)
        {
            EnsureInitialized();

            if (_currentMode == mode)
                return;

            if (!_inputs.TryGetValue(mode, out var input))
            {
                Debug.LogWarning($"PlayerInputMode {mode} not registered!");
                return;
            }

            _currentMode = mode;
            _currentInput = input;
        }
    }
}
