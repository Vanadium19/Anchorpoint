using System;
using BaseModule;

namespace MenuModule
{
    [Obsolete("Use IPauseManager instead.")]
    public interface IGamePauseService : IPauseManager
    {
    }
}
