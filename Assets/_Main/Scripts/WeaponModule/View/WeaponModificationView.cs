using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Shows the objects of installed modifications and hides the rest. Also provides the aim pose
    /// required by an installed modification, e.g. a red dot sight that changes the sight line.
    /// </summary>
    public class WeaponModificationView : WeaponViewPart
    {
        [SerializeField] private WeaponModificationVisual[] visuals;

        private bool _hasAimPose;
        private Vector3 _aimPosition;
        private Vector3 _aimRotation;

        public override void OnBuildChanged(WeaponBuild build)
        {
            _hasAimPose = false;

            if (visuals == null)
                return;

            foreach (var entry in visuals)
                SetVisualsActive(entry, false);

            foreach (var modification in build.Modifications)
            {
                foreach (var entry in visuals)
                {
                    if (entry.Modification != modification)
                        continue;

                    SetVisualsActive(entry, true);
                    ResolveAimPose(entry);
                }
            }
        }

        /// <summary>
        /// Returns the aim pose of the last installed modification that overrides it.
        /// Installation order is the order of <see cref="WeaponBuild.Modifications"/>.
        /// </summary>
        public bool TryGetAimPose(out Vector3 position, out Vector3 rotation)
        {
            position = _aimPosition;
            rotation = _aimRotation;
            return _hasAimPose;
        }

        private static void SetVisualsActive(WeaponModificationVisual entry, bool isActive)
        {
            foreach (var visual in entry.Visuals)
            {
                if (visual != null)
                    visual.SetActive(isActive);
            }
        }

        private void ResolveAimPose(WeaponModificationVisual entry)
        {
            if (!entry.OverridesAimPose)
                return;

            _hasAimPose = true;
            _aimPosition = entry.AimPosition;
            _aimRotation = entry.AimRotation;
        }
    }
}
