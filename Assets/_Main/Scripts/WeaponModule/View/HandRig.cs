using System;
using UnityEngine;

namespace WeaponModule
{
    [Serializable]
    public class HandRig
    {
        [SerializeField] private Transform anchor;
        [SerializeField] private Transform upperArm;
        [SerializeField] private Transform lowerArm;
        [SerializeField] private Transform hand;
        [SerializeField] private Transform[] fingers = Array.Empty<Transform>();
        [SerializeField] private Transform[] triggerFingers = Array.Empty<Transform>();

        public Transform Anchor => anchor;
        public Transform UpperArm => upperArm;
        public Transform LowerArm => lowerArm;
        public Transform Hand => hand;
        public Transform[] Fingers => fingers;
        public Transform[] TriggerFingers => triggerFingers;
        public bool IsValid => anchor != null && upperArm != null && lowerArm != null && hand != null;

#if UNITY_EDITOR
        private static readonly string[] FingerNames =
        {
            "thumb_01", "thumb_02", "thumb_03",
            "index_finger_01", "index_finger_02", "index_finger_03",
            "middle_finger_01", "middle_finger_02", "middle_finger_03",
            "ring_finger_01", "ring_finger_02", "ring_finger_03",
            "little_finger_01", "little_finger_02", "little_finger_03"
        };

        private static readonly string[] TriggerFingerNames = { "index_finger_01", "index_finger_02", "index_finger_03" };

        public void AssignBones(Transform root, string sideSuffix, Transform defaultAnchor)
        {
            upperArm = FindChild(root, "upper_arm" + sideSuffix);
            lowerArm = FindChild(root, "lower_arm" + sideSuffix);
            hand = FindChild(root, "hand" + sideSuffix);
            fingers = Array.ConvertAll(FingerNames, name => FindChild(root, name + sideSuffix));
            triggerFingers = Array.ConvertAll(TriggerFingerNames, name => FindChild(root, name + sideSuffix));

            if (anchor == null)
                anchor = defaultAnchor;
        }

        private static Transform FindChild(Transform parent, string childName)
        {
            if (parent.name == childName)
                return parent;

            foreach (Transform child in parent)
            {
                var found = FindChild(child, childName);

                if (found != null)
                    return found;
            }

            return null;
        }
#endif
    }
}
