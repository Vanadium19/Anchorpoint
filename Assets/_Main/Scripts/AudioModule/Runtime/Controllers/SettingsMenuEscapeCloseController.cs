using System;
using InputModule;
using Zenject;

namespace AudioModule
{
    public class SettingsMenuEscapeCloseController : ITickable
    {
        private readonly SettingsMenuView _view;
        private readonly IInputMap _inputMap;

        public SettingsMenuEscapeCloseController(SettingsMenuView view,
            IInputMap inputMap)
        {
            _view = view ?? throw new ArgumentNullException(nameof(SettingsMenuView));
            _inputMap = inputMap ?? throw new ArgumentNullException(nameof(inputMap));
        }

        public void Tick()
        {
            if (!_view.IsVisible || !_inputMap.IsPausePressed)
                return;

            _view.Hide();
        }
    }
}