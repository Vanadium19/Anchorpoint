using BaseModule;
using ComponentsModule;
using UnityEngine;
using UnityEngine.AI;
using UtilsModule;

namespace EnvironmentModule
{
    public sealed class DoorView : MonoBehaviour, IInteractable
    {
        public enum OpenMode
        {
            Automatic,
            Manual
        }

        public enum ManualDirection
        {
            OpenAngle,
            OppositeOpenAngle
        }

        private enum DoorState
        {
            Closed,
            OpenPositive,
            OpenNegative,
            Partial
        }

        private const float AngleEpsilon = 0.5f;

        [SerializeField] private Transform hinge;

        [Header("Angles (local Y)")]
        [SerializeField] private float closedAngle;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float oppositeOpenAngle = -90f;
        [SerializeField] private float openDuration = 0.6f;

        [Header("Mode")]
        [SerializeField] private OpenMode openMode = OpenMode.Automatic;
        [SerializeField] private ManualDirection manualDirection = ManualDirection.OpenAngle;
        [Tooltip("Partially open door: closer to closed opens it, closer to open closes it.")]
        [SerializeField] private bool invertPartialLogic = true;

        [Header("Automatic")]
        [SerializeField] private Vector3 leafLocalDirection = Vector3.right;
        [SerializeField] private float openingRadius = 1f;

        [Header("Interaction")]
        [SerializeField] private string displayName = "Door";
        [SerializeField] private string nameKey = "door_name";

        [Header("Navigation")]
        [SerializeField] private NavMeshObstacle blocker;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;

        private Vector3 _baseEuler;
        private float _positiveOpenAngle;
        private float _negativeOpenAngle;
        private float _manualOpenAngle;
        private float _currentAngle;
        private float _startAngle;
        private float _targetAngle;
        private float _deltaAngle;
        private float _elapsed;
        private float _duration;
        private bool _isAnimating;
        private bool _isPaused;
        private DoorState _state;

        public string DisplayName => string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);

        public string HintKey
        {
            get
            {
                if (_state == DoorState.OpenPositive || _state == DoorState.OpenNegative)
                    return InteractionHintKeys.Close;

                if (_state == DoorState.Partial)
                {
                    var nearestIsClosed = NearestTargetIsClosed();
                    if (invertPartialLogic)
                        nearestIsClosed = !nearestIsClosed;

                    return nearestIsClosed ? InteractionHintKeys.Close : InteractionHintKeys.Open;
                }

                return InteractionHintKeys.Open;
            }
        }

        private void Awake()
        {
            if (hinge == null)
                hinge = transform;

            _baseEuler = hinge.localEulerAngles;
            CacheOpenAngles();
            _manualOpenAngle = manualDirection == ManualDirection.OpenAngle ? openAngle : oppositeOpenAngle;

            _currentAngle = Mathf.DeltaAngle(0f, hinge.localEulerAngles.y);
            _state = ClassifyAngle(_currentAngle);

            ApplyBlocker();
        }

        private void OnEnable()
        {
            PauseState.PauseChanged += OnPauseChanged;
            _isPaused = PauseState.IsPaused;
        }

        private void OnDisable() => PauseState.PauseChanged -= OnPauseChanged;

        private void Update()
        {
            if (_isPaused || !_isAnimating)
                return;

            _elapsed += Time.deltaTime;
            var progress = _duration <= 0f ? 1f : Mathf.Clamp01(_elapsed / _duration);
            var eased = Mathf.SmoothStep(0f, 1f, progress);
            _currentAngle = _startAngle + _deltaAngle * eased;
            ApplyAngle();

            if (progress < 1f)
                return;

            _currentAngle = _targetAngle;
            _isAnimating = false;
            ApplyAngle();
        }

        public bool CanInteract(Transform interactor) => interactor != null;

        public void Interact(Transform interactor)
        {
            switch (_state)
            {
                case DoorState.Closed:
                    Open(interactor);
                    break;

                case DoorState.OpenPositive:
                case DoorState.OpenNegative:
                    Close();
                    break;

                default:
                    SnapToNearest();
                    break;
            }
        }

        private void Open(Transform interactor)
        {
            if (!TryChooseOpenAngle(interactor, out var targetAngle))
                return;

            var positive = Mathf.DeltaAngle(closedAngle, targetAngle) >= 0f;
            _state = positive ? DoorState.OpenPositive : DoorState.OpenNegative;

            StartAnimation(targetAngle);
            ApplyBlocker();
            PlaySound(openClip);
        }

        private void Close()
        {
            _state = DoorState.Closed;

            StartAnimation(closedAngle);
            ApplyBlocker();
            PlaySound(closeClip);
        }

        private void SnapToNearest()
        {
            if (openMode == OpenMode.Manual)
            {
                SnapToNearestManual();
                return;
            }

            var toClosed = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, closedAngle));
            var toPositive = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _positiveOpenAngle));
            var toNegative = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _negativeOpenAngle));

            var nearestIsClosed = toClosed <= toPositive && toClosed <= toNegative;
            if (invertPartialLogic)
                nearestIsClosed = !nearestIsClosed;

            if (nearestIsClosed)
            {
                _state = DoorState.Closed;
                StartAnimation(closedAngle);
                ApplyBlocker();
                PlaySound(closeClip);
                return;
            }

            if (toPositive <= toNegative)
            {
                _state = DoorState.OpenPositive;
                StartAnimation(_positiveOpenAngle);
                ApplyBlocker();
                PlaySound(openClip);
                return;
            }

            _state = DoorState.OpenNegative;
            StartAnimation(_negativeOpenAngle);
            ApplyBlocker();
            PlaySound(openClip);
        }

        private void SnapToNearestManual()
        {
            var toClosed = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, closedAngle));
            var toManual = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _manualOpenAngle));

            var nearestIsClosed = toClosed <= toManual;
            if (invertPartialLogic)
                nearestIsClosed = !nearestIsClosed;

            if (nearestIsClosed || !CanOpen(_manualOpenAngle))
            {
                _state = DoorState.Closed;
                StartAnimation(closedAngle);
                ApplyBlocker();
                PlaySound(closeClip);
                return;
            }

            var positive = Mathf.DeltaAngle(closedAngle, _manualOpenAngle) >= 0f;
            _state = positive ? DoorState.OpenPositive : DoorState.OpenNegative;
            StartAnimation(_manualOpenAngle);
            ApplyBlocker();
            PlaySound(openClip);
        }

        private bool TryChooseOpenAngle(Transform interactor, out float targetAngle)
        {
            targetAngle = openMode == OpenMode.Manual
                ? (manualDirection == ManualDirection.OpenAngle ? openAngle : oppositeOpenAngle)
                : ChooseAutomaticAngle(interactor);

            return CanOpen(targetAngle);
        }

        private float ChooseAutomaticAngle(Transform interactor)
        {
            var playerOnPositiveSide = IsPlayerOnPositiveSide(interactor);
            var pushIsPositive = playerOnPositiveSide;
            var pullIsPositive = !playerOnPositiveSide;

            var pushAngle = pushIsPositive ? _positiveOpenAngle : _negativeOpenAngle;
            var pullAngle = pullIsPositive ? _positiveOpenAngle : _negativeOpenAngle;

            if (!CanOpen(pullAngle))
                return pushAngle;

            if (!CanOpen(pushAngle))
                return pullAngle;

            return IsPlayerInOpeningArea(interactor, pullAngle, pullIsPositive)
                ? pushAngle
                : pullAngle;
        }

        private Vector3 GetLeafDirection()
        {
            var leafDirection = hinge.rotation * leafLocalDirection;
            leafDirection.y = 0f;
            return leafDirection;
        }

        private bool IsPlayerOnPositiveSide(Transform interactor)
        {
            if (interactor == null)
                return true;

            var offset = interactor.position - hinge.position;
            offset.y = 0f;

            var normal = Vector3.Cross(GetLeafDirection(), Vector3.up);

            return Vector3.Dot(offset, normal) >= 0f;
        }

        private bool IsPlayerInOpeningArea(Transform interactor, float pullAngle, bool pullIsPositive)
        {
            if (interactor == null)
                return false;

            var offset = interactor.position - hinge.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > openingRadius * openingRadius)
                return false;

            var leafDirection = GetLeafDirection();

            if (leafDirection.sqrMagnitude <= 0.0001f)
                return false;

            var signedAngle = Vector3.SignedAngle(leafDirection, offset, Vector3.up);
            var pullMagnitude = Mathf.Abs(Mathf.DeltaAngle(closedAngle, pullAngle));

            return pullIsPositive
                ? signedAngle >= 0f && signedAngle <= pullMagnitude
                : signedAngle <= 0f && -signedAngle <= pullMagnitude;
        }

        private bool CanOpen(float angle) =>
            Mathf.Abs(Mathf.DeltaAngle(closedAngle, angle)) > AngleEpsilon;

        private void StartAnimation(float targetAngle)
        {
            var distance = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, targetAngle));

            if (distance <= AngleEpsilon || openDuration <= 0f)
            {
                _currentAngle = targetAngle;
                _isAnimating = false;
                ApplyAngle();
                return;
            }

            var maxSwing = Mathf.Max(
                Mathf.Abs(Mathf.DeltaAngle(closedAngle, _positiveOpenAngle)),
                Mathf.Abs(Mathf.DeltaAngle(closedAngle, _negativeOpenAngle)));

            _startAngle = _currentAngle;
            _targetAngle = targetAngle;
            _deltaAngle = Mathf.DeltaAngle(_startAngle, _targetAngle);
            _elapsed = 0f;
            _duration = openDuration * Mathf.Clamp01(distance / Mathf.Max(maxSwing, AngleEpsilon));
            _isAnimating = true;
        }

        private void ApplyAngle()
        {
            var euler = _baseEuler;
            euler.y = _currentAngle;
            hinge.localEulerAngles = euler;
        }

        private void ApplyBlocker()
        {
            if (blocker != null)
                blocker.enabled = _state == DoorState.Closed;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void CacheOpenAngles()
        {
            if (Mathf.DeltaAngle(closedAngle, openAngle) >= 0f)
            {
                _positiveOpenAngle = openAngle;
                _negativeOpenAngle = oppositeOpenAngle;
                return;
            }

            _positiveOpenAngle = oppositeOpenAngle;
            _negativeOpenAngle = openAngle;
        }

        private DoorState ClassifyAngle(float angle)
        {
            if (IsNear(angle, closedAngle))
                return DoorState.Closed;

            if (IsNear(angle, _positiveOpenAngle))
                return DoorState.OpenPositive;

            if (IsNear(angle, _negativeOpenAngle))
                return DoorState.OpenNegative;

            return DoorState.Partial;
        }

        private bool NearestTargetIsClosed()
        {
            var toClosed = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, closedAngle));

            if (openMode == OpenMode.Manual)
                return toClosed <= Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _manualOpenAngle));

            var toPositive = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _positiveOpenAngle));
            var toNegative = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _negativeOpenAngle));

            return toClosed <= toPositive && toClosed <= toNegative;
        }

        private static bool IsNear(float angle, float target) =>
            Mathf.Abs(Mathf.DeltaAngle(angle, target)) <= AngleEpsilon;

        private void OnPauseChanged(bool isPaused) => _isPaused = isPaused;
    }
}
