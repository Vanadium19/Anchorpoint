using System;
using BaseModule;

namespace MenuModule
{
    [Obsolete("Use PauseManager instead.")]
    public class GamePauseService : PauseManager, IGamePauseService
    {
    }
}
