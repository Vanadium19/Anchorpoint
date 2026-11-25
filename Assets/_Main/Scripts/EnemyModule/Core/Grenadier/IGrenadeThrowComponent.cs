using System;
using UnityEngine;

namespace EnemyModule
{
    /// <summary>
    /// Throws grenades: the throw starts with an animation and the grenade leaves the hand later.
    /// </summary>
    public interface IGrenadeThrowComponent
    {
        /// <summary>
        /// Raised when a throw begins, so the view can play the throwing animation.
        /// </summary>
        event Action ThrowStarted;

        /// <summary>
        /// Raised when a grenade leaves the hand.
        /// </summary>
        event Action ThrowReleased;

        /// <summary>
        /// Announces the start of a throw.
        /// </summary>
        void BeginThrow();

        /// <summary>
        /// Lets go of a grenade that lands at <paramref name="targetPosition"/>.
        /// </summary>
        void Release(Vector3 targetPosition);
    }
}
