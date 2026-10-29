using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A mechanism of a weapon view (hand grip, visible rounds, moving parts, effects).
    /// WeaponView forwards weapon state changes to every part on its prefab, so a new mechanism
    /// is added as a new part without changing WeaponView. Override only the handlers the part needs.
    /// </summary>
    public abstract class WeaponViewPart : MonoBehaviour
    {
        /// <summary>
        /// Offset added to the aim position of the weapon config, e.g. to look through an installed sight.
        /// </summary>
        public virtual Vector3 AimPositionOffset => Vector3.zero;

        public virtual void Initialize(WeaponConfig config)
        {
        }

        public virtual void OnTriggerHeld(bool isHeld)
        {
        }

        public virtual void OnFired()
        {
        }

        public virtual void OnActionCycleStarted()
        {
        }

        public virtual void OnReloadStarted(bool isEmptyReload)
        {
        }

        public virtual void OnReloadStateChanged(bool isReloading)
        {
        }

        public virtual void OnRoundInserted()
        {
        }

        public virtual void OnLoadedRoundsChanged(int count)
        {
        }

        public virtual void OnHolsterStateChanged(bool isHolstered)
        {
        }

        public virtual void OnPaused(bool isPaused)
        {
        }

        /// <summary>
        /// Called for every Animation Event raised by the weapon clips, identified by its id.
        /// </summary>
        public virtual void OnAnimationEvent(string eventId)
        {
        }

        public virtual void ResetVisuals()
        {
        }

        /// <summary>
        /// Called when the set of installed modifications of the weapon changes.
        /// </summary>
        public virtual void OnModificationsChanged(IReadOnlyList<WeaponModificationConfig> modifications)
        {
        }
    }
}
