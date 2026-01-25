using System;
using UnityEngine;

namespace ComponentsModule
{
    [Serializable]
    public class CrouchData
    {
        [SerializeField] private float crouchHeight = 1.1f;
        [SerializeField] private float standHeight = 1.5f;

        [SerializeField] private float eyeHeightCrouching = 0.8f;
        [SerializeField] private float eyeHeightStanding = 1.2f;

        [SerializeField] private float crouchSpeed = 10;

        public float CrouchHeight => crouchHeight;
        public float StandHeight => standHeight;

        public float EyeHeightCrouching => eyeHeightCrouching;
        public float EyeHeightStanding => eyeHeightStanding;

        public float CrouchSpeed => crouchSpeed;
    }
}