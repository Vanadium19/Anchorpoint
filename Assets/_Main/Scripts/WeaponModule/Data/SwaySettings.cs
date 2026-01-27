using UnityEngine;

namespace WeaponModule
{
    [System.Serializable]
    public class SwaySettings
    {
        [Header("Position")]
        public float Step = 0.02f;
        public float MaxStep = 0.06f;
        public float Smooth = 8f;

        [Header("Rotation")]
        public float RotationStep = 4f;
        public float MaxRotation = 10f;
        public float SmoothRot = 10f;

        [Header("Tilt (Z)")]
        public float Tilt = 2f;
        public float MaxTilt = 5f;
    }
}