using System;
using UnityEngine;

namespace ComponentsModule
{
    /// <summary>
    /// Announces shots fired by the player so other entities can react to being aimed at.
    /// </summary>
    public interface IShotNotifier
    {
        /// <summary>
        /// Raised once per shot with the world-space origin and normalized direction of the shot.
        /// </summary>
        event Action<Vector3, Vector3> ShotFired;
    }
}
