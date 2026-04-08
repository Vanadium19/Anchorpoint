using UnityEngine;

namespace SharedData
{
    public static class WeaponAnimatorHashes
    {
        public static readonly int TriggerHold = Animator.StringToHash("TriggerHold");
        public static readonly int FireState = Animator.StringToHash("Fire");
        public static readonly int Reload = Animator.StringToHash("Reload");
        public static readonly int EmptyReload = Animator.StringToHash("EmptyReload");
        public static readonly int Holster = Animator.StringToHash("Holster");
        public static readonly int Draw = Animator.StringToHash("Draw");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int InputX = Animator.StringToHash("InputX");
        public static readonly int InputY = Animator.StringToHash("InputY");
        public static readonly int AimBlend = Animator.StringToHash("AimBlend");
    }
}