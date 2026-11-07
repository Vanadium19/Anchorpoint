using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WeaponModule
{
    public class WeaponGripView : WeaponViewPart
    {
        private const float GizmoRadius = 0.01f;

        [SerializeField] private WeaponGripConfig config;
        [SerializeField] private HandRig rightHand = new();
        [SerializeField] private HandRig leftHand = new();
        [SerializeField] private Animator handsAnimator;
        [SerializeField] private Animator gunAnimator;

        [Header("Blending")]
        [SerializeField, Min(0f)] private float releaseBlendSpeed = 8f;
        [SerializeField, Min(0.001f)] private float triggerPressDuration = 0.05f;
        [SerializeField, Min(0.001f)] private float triggerReleaseDuration = 0.12f;

        private float _rightWeight = 1f;
        private float _leftWeight = 1f;
        private float _triggerWeight;
        private bool _isReloading;
        private bool _isTriggerHeld;
        private bool _isTriggerPressPending;
        private bool _isPaused;
        private bool _isPoseEditing;

        private void LateUpdate()
        {
            if (config == null || _isPoseEditing)
                return;

            if (!_isPaused)
                UpdateWeights(Time.deltaTime);

            ApplyHand(rightHand, config.RightHand, _rightWeight);
            ApplyHand(leftHand, config.LeftHand, _leftWeight);
            ApplyTriggerPress(_rightWeight * _triggerWeight);
        }

        private void OnDrawGizmosSelected()
        {
            if (config == null)
                return;

            DrawGripGizmo(rightHand, config.RightHand, Color.red);
            DrawGripGizmo(leftHand, config.LeftHand, Color.blue);
        }

        public override void OnTriggerHeld(bool isHeld)
        {
            if (isHeld && !_isTriggerHeld)
                _isTriggerPressPending = true;

            _isTriggerHeld = isHeld;
        }

        public override void OnReloadStateChanged(bool isReloading) => _isReloading = isReloading;

        public override void OnPaused(bool isPaused) => _isPaused = isPaused;

        public override void ResetVisuals()
        {
            _isReloading = false;
            _isTriggerHeld = false;
            _isTriggerPressPending = false;
            _triggerWeight = 0f;

            if (config == null)
                return;

            _rightWeight = GetTargetWeight(config.RightHand);
            _leftWeight = GetTargetWeight(config.LeftHand);
        }

        private void UpdateWeights(float deltaTime)
        {
            var releaseStep = releaseBlendSpeed * deltaTime;
            _rightWeight = Mathf.MoveTowards(_rightWeight, GetTargetWeight(config.RightHand), releaseStep);
            _leftWeight = Mathf.MoveTowards(_leftWeight, GetTargetWeight(config.LeftHand), releaseStep);

            var triggerTarget = _isTriggerHeld || _isTriggerPressPending ? 1f : 0f;
            var triggerDuration = triggerTarget > _triggerWeight ? triggerPressDuration : triggerReleaseDuration;
            _triggerWeight = Mathf.MoveTowards(_triggerWeight, triggerTarget, deltaTime / triggerDuration);

            if (_triggerWeight >= 1f)
                _isTriggerPressPending = false;
        }

        private float GetTargetWeight(HandGripData grip) => _isReloading && grip.IsReleasedDuringReload ? 0f : 1f;

        private static void ApplyHand(HandRig rig, HandGripData grip, float weight)
        {
            if (!grip.IsEnabled || !rig.IsValid || weight <= 0f)
                return;

            var targetPosition = rig.Anchor.TransformPoint(grip.GripPosition);
            var targetRotation = rig.Anchor.rotation * grip.GripRotation;
            TwoBoneArmSolver.Solve(rig.UpperArm, rig.LowerArm, rig.Hand, targetPosition, targetRotation, weight);
            ApplyRotations(rig.Fingers, grip.FingerRotations, weight);
        }

        private void ApplyTriggerPress(float weight)
        {
            if (config == null || !config.RightHand.IsEnabled || weight <= 0f)
                return;

            ApplyRotations(rightHand.TriggerFingers, config.RightHand.TriggerPressRotations, weight);
        }

        private static void ApplyRotations(Transform[] bones, Quaternion[] rotations, float weight)
        {
            var count = Mathf.Min(bones.Length, rotations.Length);

            for (var i = 0; i < count; i++)
            {
                if (bones[i] != null)
                    bones[i].localRotation = Quaternion.Slerp(bones[i].localRotation, rotations[i], weight);
            }
        }

        private static void DrawGripGizmo(HandRig rig, HandGripData grip, Color color)
        {
            if (!grip.IsEnabled || rig.Anchor == null)
                return;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(rig.Anchor.TransformPoint(grip.GripPosition), GizmoRadius);
        }

#if UNITY_EDITOR
        [FoldoutGroup("Pose Editing"), Button]
        private void AssignBones()
        {
            Undo.RecordObject(this, "Assign Grip Bones");
            var defaultAnchor = FindAnchor(transform);
            rightHand.AssignBones(transform, "_R", defaultAnchor);
            leftHand.AssignBones(transform, "_L", defaultAnchor);
            EditorUtility.SetDirty(this);
        }

        [FoldoutGroup("Pose Editing"), Button, DisableIf(nameof(_isPoseEditing))]
        private void BeginPoseEditing() => SetPoseEditing(true);

        [FoldoutGroup("Pose Editing"), Button, EnableIf(nameof(_isPoseEditing))]
        private void EndPoseEditing() => SetPoseEditing(false);

        private void SetPoseEditing(bool isPoseEditing)
        {
            _isPoseEditing = isPoseEditing;

            if (handsAnimator != null)
                handsAnimator.enabled = !isPoseEditing;

            if (gunAnimator != null)
                gunAnimator.enabled = !isPoseEditing;
        }

        [FoldoutGroup("Pose Editing"), Button]
        private void CaptureRightHandGrip() => CaptureGrip(rightHand, config.RightHand);

        [FoldoutGroup("Pose Editing"), Button]
        private void CaptureLeftHandGrip() => CaptureGrip(leftHand, config.LeftHand);

        [FoldoutGroup("Pose Editing"), Button]
        private void CaptureTriggerPressPose()
        {
            if (!CanCapture(rightHand))
                return;

            Undo.RecordObject(config, "Capture Trigger Press Pose");
            config.RightHand.SetTriggerPressRotations(ReadLocalRotations(rightHand.TriggerFingers));
            SaveConfig();
        }

        private void CaptureGrip(HandRig rig, HandGripData grip)
        {
            if (!CanCapture(rig))
                return;

            Undo.RecordObject(config, "Capture Hand Grip");
            var gripPosition = rig.Anchor.InverseTransformPoint(rig.Hand.position);
            var gripRotation = Quaternion.Inverse(rig.Anchor.rotation) * rig.Hand.rotation;
            grip.SetGrip(gripPosition, gripRotation, ReadLocalRotations(rig.Fingers));
            SaveConfig();
        }

        private bool CanCapture(HandRig rig)
        {
            if (config != null && rig.IsValid)
                return true;

            Debug.LogWarning($"{name}: assign the grip config and bones before capturing a pose.", this);
            return false;
        }

        private void SaveConfig()
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
        }

        private static Quaternion[] ReadLocalRotations(Transform[] bones)
        {
            var rotations = new Quaternion[bones.Length];

            for (var i = 0; i < bones.Length; i++)
                rotations[i] = bones[i] != null ? bones[i].localRotation : Quaternion.identity;

            return rotations;
        }

        private static Transform FindAnchor(Transform parent)
        {
            if (parent.name == "gun_root_bone")
                return parent;

            foreach (Transform child in parent)
            {
                var found = FindAnchor(child);

                if (found != null)
                    return found;
            }

            return null;
        }
#endif
    }
}
