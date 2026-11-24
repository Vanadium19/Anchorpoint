using System;
using ComponentsModule;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Publishes the player's shots through <see cref="IShotNotifier"/>.
    /// </summary>
    public sealed class PlayerShotNotifier : IShotNotifier
    {
        public event Action<Vector3, Vector3> ShotFired;

        /// <summary>
        /// Announces a shot to everyone listening.
        /// </summary>
        public void Notify(Vector3 origin, Vector3 direction) => ShotFired?.Invoke(origin, direction.normalized);
    }
}
