using UnityEngine;

namespace WeaponModule
{
    [CreateAssetMenu(fileName = "WeaponGripConfig", menuName = "Configs/Weapon Grip")]
    public class WeaponGripConfig : ScriptableObject
    {
        [SerializeField] private HandGripData rightHand = new();
        [SerializeField] private HandGripData leftHand = new();

        public HandGripData RightHand => rightHand;
        public HandGripData LeftHand => leftHand;
    }
}
