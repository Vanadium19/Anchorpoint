using System;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Receiver of the Animation Events of weapon clips. A clip has one kind of event, <c>RaiseEvent</c>,
    /// whose data is a weapon event id that every part of the weapon can react to.
    /// </summary>
    public class WeaponAnimationEvents : MonoBehaviour
    {
        public event Action<string> EventRaised;

        private void RaiseEvent(string eventId) => EventRaised?.Invoke(eventId);
    }
}
