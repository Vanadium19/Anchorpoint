using System;
using UnityEngine;

namespace WeaponModule
{
    [Serializable]
    public class HandGripData
    {
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool isReleasedDuringReload;
        [SerializeField] private Vector3 gripPosition;
        [SerializeField] private Quaternion gripRotation = Quaternion.identity;
        [SerializeField] private Quaternion[] fingerRotations = Array.Empty<Quaternion>();
        [SerializeField] private Quaternion[] triggerPressRotations = Array.Empty<Quaternion>();

        public bool IsEnabled => isEnabled;
        public bool IsReleasedDuringReload => isReleasedDuringReload;
        public Vector3 GripPosition => gripPosition;
        public Quaternion GripRotation => gripRotation;
        public Quaternion[] FingerRotations => fingerRotations;
        public Quaternion[] TriggerPressRotations => triggerPressRotations;

        public void SetGrip(Vector3 position, Quaternion rotation, Quaternion[] fingers)
        {
            gripPosition = position;
            gripRotation = rotation;
            fingerRotations = fingers ?? Array.Empty<Quaternion>();
            isEnabled = true;
        }

        public void SetTriggerPressRotations(Quaternion[] rotations) =>
            triggerPressRotations = rotations ?? Array.Empty<Quaternion>();

        public void SetReleasedDuringReload(bool isReleased) => isReleasedDuringReload = isReleased;
    }
}
