using System;
using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponView : MonoBehaviour
    {
        private const string GunClearLayerName = "Clear";
        private const float ClearCrossFadeTime = 0.08f;

        private static readonly int ArmsClearStateHash = Animator.StringToHash("ClearMalfunction");
        private static readonly int GunClearStateHash = Animator.StringToHash("ClearMisfire");

        [Header("References")]
        [SerializeField] private Animator handsAnimator;
        [SerializeField] private Animator gunAnimator;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPivot;

        [Header("Animation")]
        [SerializeField] private int fireAnimationLayer = 1;
        [Tooltip("Arms clips whose length is stretched to the reload time of the config. Optional.")]
        [SerializeField] private AnimationClip reloadClip;
        [SerializeField] private AnimationClip emptyReloadClip;
        [SerializeField] private AnimationClip roundReloadClip;
        [Tooltip("Clips of clearing a malfunction, stretched to the clear time of the config. Optional.")]
        [SerializeField] private AnimationClip clearMisfireClip;

        [Header("Malfunction Clearing")]
        [Tooltip("Clip: the clear clip above. ActionCycle: the mechanism animation of a shot, repeated. ReloadAnimation: the reload animation.")]
        [SerializeField] private MalfunctionClearStyle misfireClearStyle = MalfunctionClearStyle.Clip;
        [SerializeField] private MalfunctionClearStyle jamClearStyle = MalfunctionClearStyle.Clip;
        [Min(1)]
        [SerializeField] private int misfireActionCycles = 1;
        [Min(1)]
        [SerializeField] private int jamActionCycles = 1;
        [Tooltip("Releases the grip of the hands so the arms clip of the clear animation drives them.")]
        [SerializeField] private bool releaseHandsDuringClear = true;
        [Tooltip("Plays the arms clip of the clear animation. Off when the hands only hold the weapon.")]
        [SerializeField] private bool playArmsClearClip = true;
        [Tooltip("The clear clip never plays faster than this, even when the configured clear time is shorter.")]
        [Min(1f)]
        [SerializeField] private float maxClearSpeed = 1.5f;

        [Header("Shake")]
        [Tooltip("Shake of the whole weapon, started by the ShakeStarted Animation Event of a clear clip.")]
        [Min(0.01f)]
        [SerializeField] private float shakeDuration = 0.7f;
        [Tooltip("Rotation amplitude in degrees. Zero shakes the weapon without rotating it.")]
        [SerializeField] private float shakeAngle = 7f;
        [SerializeField] private float shakeDistance = 0.025f;
        [Tooltip("Weight of the shake along each axis of the recoil pivot.")]
        [SerializeField] private Vector3 shakeAxis = new(1f, 0.5f, 0f);
        [Tooltip("Frequency of the vertical shake relative to the horizontal one. One shakes up and down in one rhythm.")]
        [Min(0f)]
        [SerializeField] private float shakeVerticalFrequencyScale = 1.7f;
        [Min(0f)]
        [SerializeField] private float shakeFrequency = 6f;
        [Tooltip("Seconds the trigger stays pressed after a TriggerPulled Animation Event of a clear clip.")]
        [Min(0.01f)]
        [SerializeField] private float triggerPullDuration = 0.09f;

        [Header("Recoil Targets")]
        [SerializeField] private Transform recoilPivot;
        [SerializeField] private GameObject bulletPrefab;

        [Header("Mechanism")]
        [SerializeField] private WeaponAnimationEvents animationEvents;

        private readonly RecoilProcessor _weaponRecoil = new();
        private readonly SwayProcessor _sway = new();

        private ICameraRecoilService _cameraRecoil;
        private WeaponViewPart[] _parts;
        private Vector3 _hipPosition;
        private Quaternion _hipRotation;
        private WeaponConfig _config;
        private float _currentAimBlend;
        private Transform _cameraTransform;
        private bool _isPaused;
        private float _shakeElapsed = float.MaxValue;
        private float _triggerPullRemaining;

        public GameObject BulletPrefab => bulletPrefab;
        public Transform FirePoint => firePoint;

        private WeaponViewPart[] Parts => _parts ??= GetComponentsInChildren<WeaponViewPart>(true);

        private void OnEnable()
        {
            if (animationEvents != null)
                animationEvents.EventRaised += OnAnimationEventRaised;
        }

        private void OnDisable()
        {
            if (animationEvents != null)
                animationEvents.EventRaised -= OnAnimationEventRaised;
        }

        [Inject]
        private void Construct(ICameraRecoilService cameraRecoil) => _cameraRecoil = cameraRecoil;

        public void Initialize(WeaponConfig config)
        {
            _config = config;

            if (aimPivot != null)
            {
                _hipPosition = aimPivot.localPosition;
                _hipRotation = aimPivot.localRotation;
            }

            var mainCamera = Camera.main;
            _cameraTransform = mainCamera != null ? mainCamera.transform.parent : null;

            ForEachPart(static (part, weaponConfig) => part.Initialize(weaponConfig), config);
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            SetAnimatorSpeed(isPaused ? 0f : 1f);
            ForEachPart(static (part, paused) => part.OnPaused(paused), isPaused);
        }

        public void SetTriggerHold(bool isHeld)
        {
            isHeld |= _triggerPullRemaining > 0f;

            SetBool(WeaponAnimatorHashes.TriggerHold, isHeld);
            ForEachPart(static (part, held) => part.OnTriggerHeld(held), isHeld);
        }

        public void PlayFireEffects(bool isAiming)
        {
            PlayFireAnimation();

            if (isAiming)
            {
                _weaponRecoil.Fire(_config.AimRecoil);
                _cameraRecoil.FireCamera(_config.CamAimRecoil);
            }
            else
            {
                _weaponRecoil.Fire(_config.HipRecoil);
                _cameraRecoil.FireCamera(_config.CamHipRecoil);
            }

            ForEachPart(static part => part.OnFired());
            PlayActionCycle();
        }

        public void PlayRoundInserted() => ForEachPart(static part => part.OnRoundInserted());

        public void SetLoadedRounds(int count) =>
            ForEachPart(static (part, loadedCount) => part.OnLoadedRoundsChanged(loadedCount), count);

        public void PlayActionCycle()
        {
            PlayState(WeaponAnimatorHashes.ActionCycleState);
            ForEachPart(static part => part.OnActionCycleStarted());
        }

        public void SetReloadState(bool isReloading)
        {
            ForEachPart(static (part, reloading) => part.OnReloadStateChanged(reloading), isReloading);

            if (!isReloading)
                SetBool(WeaponAnimatorHashes.IsRoundReloading, false);
        }

        public void PlayReload(bool isEmptyReload, float durationOverride = 0f)
        {
            ForEachPart(static (part, isEmpty) => part.OnReloadStarted(isEmpty), isEmptyReload);

            if (_config.ReloadMode == ReloadMode.SingleRound)
            {
                SetFloat(WeaponAnimatorHashes.ReloadSpeed, GetReloadSpeed(roundReloadClip, durationOverride > 0f ? durationOverride : _config.RoundReloadTime));
                SetBool(WeaponAnimatorHashes.IsRoundReloading, true);
                return;
            }

            var isEmptyAnimation = isEmptyReload && _config.UseEmptyReloadAnimation;
            var clip = isEmptyAnimation ? emptyReloadClip : reloadClip;

            SetFloat(WeaponAnimatorHashes.ReloadSpeed, GetReloadSpeed(clip, durationOverride > 0f ? durationOverride : _config.ReloadTime));
            SetTrigger(isEmptyAnimation ? WeaponAnimatorHashes.EmptyReload : WeaponAnimatorHashes.Reload);
        }

        public void PlayMalfunction(WeaponMalfunction malfunction) =>
            ForEachPart(static (part, kind) => part.OnMalfunctionOccurred(kind), malfunction);

        public MalfunctionClearStyle GetClearStyle(WeaponMalfunction malfunction) =>
            malfunction == WeaponMalfunction.Jam ? jamClearStyle : misfireClearStyle;

        public int GetActionCycles(WeaponMalfunction malfunction) =>
            Mathf.Max(1, malfunction == WeaponMalfunction.Jam ? jamActionCycles : misfireActionCycles);

        public void PlayMalfunctionClear(WeaponMalfunction malfunction, float duration)
        {
            ForEachPart(static (part, kind) => part.OnMalfunctionClearStarted(kind), malfunction);

            switch (GetClearStyle(malfunction))
            {
                case MalfunctionClearStyle.ReloadAnimation:
                    PlayReload(true, duration);
                    break;
                case MalfunctionClearStyle.ActionCycle:
                    PlayActionCycle();
                    break;
                default:
                    SetFloat(WeaponAnimatorHashes.ReloadSpeed, GetReloadSpeed(clearMisfireClip, duration));
                    PlayClearClip();
                    break;
            }
        }

        public float GetClearClipLength(WeaponMalfunction malfunction) =>
            GetClearStyle(malfunction) == MalfunctionClearStyle.Clip && clearMisfireClip != null ? clearMisfireClip.length : 0f;

        public float MaxClearSpeed => maxClearSpeed;

        public bool ShouldReleaseHandsDuringClear => releaseHandsDuringClear;

        public void SetHolsterState(bool isHolstered)
        {
            SetTrigger(isHolstered ? WeaponAnimatorHashes.Holster : WeaponAnimatorHashes.Draw);
            ForEachPart(static (part, holstered) => part.OnHolsterStateChanged(holstered), isHolstered);
        }

        public void SetMovementState(bool isMoving, Vector2 inputVector)
        {
            SetBool(WeaponAnimatorHashes.IsMoving, isMoving);
            SetFloat(WeaponAnimatorHashes.InputX, inputVector.x, 0.1f);
            SetFloat(WeaponAnimatorHashes.InputY, inputVector.y, 0.1f);
        }

        public void UpdateAiming(bool isAiming, float stabilityTarget)
        {
            if (aimPivot == null)
                return;

            var targetPosition = isAiming ? _config.AimPosition : _hipPosition;
            var targetRotation = isAiming ? Quaternion.Euler(_config.AimRotation) : _hipRotation;
            var speed = _config.AimSpeed * Time.deltaTime;

            aimPivot.localPosition = Vector3.Lerp(aimPivot.localPosition, targetPosition, speed);
            aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, targetRotation, speed);

            var targetBlend = isAiming ? stabilityTarget : 0f;
            _currentAimBlend = Mathf.Lerp(_currentAimBlend, targetBlend, Time.deltaTime * 10f);

            SetFloat(WeaponAnimatorHashes.AimBlend, _currentAimBlend);
        }

        public void UpdateProcedural(float deltaTime, Vector2 lookInput, bool isAiming)
        {
            if (_isPaused)
                return;

            _triggerPullRemaining = Mathf.Max(0f, _triggerPullRemaining - deltaTime);
            _weaponRecoil.Update(deltaTime);
            _cameraRecoil.Update(deltaTime);
            _sway.Update(lookInput, _config.Sway, deltaTime, isAiming);

            if (recoilPivot != null)
            {
                UpdateShake(deltaTime, out var shakePosition, out var shakeRotation);
                recoilPivot.localPosition = _weaponRecoil.CurrentPosition + _sway.OutputPosition + shakePosition;
                recoilPivot.localRotation = Quaternion.Euler(_weaponRecoil.CurrentRotation) * _sway.OutputRotation * shakeRotation;
            }

            if (_cameraTransform != null)
                _cameraTransform.localRotation = Quaternion.Euler(_cameraRecoil.CurrentRotation);
        }

        public void ResetVisuals()
        {
            _weaponRecoil.Reset();
            _sway.Reset();
            _currentAimBlend = 0f;
            _shakeElapsed = float.MaxValue;
            _triggerPullRemaining = 0f;

            if (aimPivot != null)
            {
                aimPivot.localPosition = _hipPosition;
                aimPivot.localRotation = _hipRotation;
            }

            ForEachPart(static part => part.ResetVisuals());

            if (handsAnimator != null)
                handsAnimator.Rebind();

            if (gunAnimator != null)
                gunAnimator.Rebind();
        }

        private void UpdateShake(float deltaTime, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;

            if (_shakeElapsed >= shakeDuration)
                return;

            _shakeElapsed += deltaTime;

            var progress = Mathf.Clamp01(_shakeElapsed / shakeDuration);
            var envelope = Mathf.Sin(progress * Mathf.PI);
            var phase = _shakeElapsed * shakeFrequency * Mathf.PI * 2f;

            position = new Vector3(
                shakeAxis.x * Mathf.Sin(phase),
                shakeAxis.y * Mathf.Sin(phase * shakeVerticalFrequencyScale + 1f),
                shakeAxis.z * Mathf.Sin(phase * 1.3f)) * (shakeDistance * envelope);
            rotation = Quaternion.Euler(
                Mathf.Sin(phase * 1.3f) * shakeAngle * 0.6f * envelope,
                Mathf.Sin(phase) * shakeAngle * 0.4f * envelope,
                Mathf.Sin(phase * 0.8f + 0.5f) * shakeAngle * envelope);
        }

        private void PlayClearClip()
        {
            if (playArmsClearClip)
                PlayClearState(handsAnimator, 0, ArmsClearStateHash);

            if (gunAnimator != null)
                PlayClearState(gunAnimator, gunAnimator.GetLayerIndex(GunClearLayerName), GunClearStateHash);
        }

        private static void PlayClearState(Animator animator, int layer, int stateHash)
        {
            if (animator == null || layer < 0 || !animator.HasState(layer, stateHash))
                return;

            animator.CrossFadeInFixedTime(stateHash, ClearCrossFadeTime, layer);
        }

        private static float GetReloadSpeed(AnimationClip clip, float duration) =>
            clip != null && duration > 0f ? clip.length / duration : 1f;

        private void PlayFireAnimation()
        {
            if (gunAnimator == null || fireAnimationLayer < 0 || fireAnimationLayer >= gunAnimator.layerCount)
                return;

            gunAnimator.Play(WeaponAnimatorHashes.FireState, fireAnimationLayer, 0f);
        }

        private void PlayState(int stateHash)
        {
            if (handsAnimator != null)
                handsAnimator.Play(stateHash, -1, 0f);

            if (gunAnimator != null)
                gunAnimator.Play(stateHash, -1, 0f);
        }

        private void SetAnimatorSpeed(float speed)
        {
            if (handsAnimator != null)
                handsAnimator.speed = speed;

            if (gunAnimator != null)
                gunAnimator.speed = speed;
        }

        private void SetBool(int hash, bool value)
        {
            if (handsAnimator != null)
                handsAnimator.SetBool(hash, value);

            if (gunAnimator != null)
                gunAnimator.SetBool(hash, value);
        }

        private void SetTrigger(int hash)
        {
            if (handsAnimator != null)
                handsAnimator.SetTrigger(hash);

            if (gunAnimator != null)
                gunAnimator.SetTrigger(hash);
        }

        private void SetFloat(int hash, float value)
        {
            if (handsAnimator != null)
                handsAnimator.SetFloat(hash, value);

            if (gunAnimator != null)
                gunAnimator.SetFloat(hash, value);
        }

        private void SetFloat(int hash, float value, float dampTime)
        {
            if (handsAnimator != null)
                handsAnimator.SetFloat(hash, value, dampTime, Time.deltaTime);

            if (gunAnimator != null)
                gunAnimator.SetFloat(hash, value, dampTime, Time.deltaTime);
        }

        private void OnAnimationEventRaised(string eventId)
        {
            if (eventId == WeaponEventIds.ShakeStarted)
                _shakeElapsed = 0f;
            else if (eventId == WeaponEventIds.TriggerPulled)
                _triggerPullRemaining = triggerPullDuration;

            ForEachPart(static (part, id) => part.OnAnimationEvent(id), eventId);
        }

        private void ForEachPart(Action<WeaponViewPart> action)
        {
            foreach (var part in Parts)
                action(part);
        }

        private void ForEachPart<T>(Action<WeaponViewPart, T> action, T argument)
        {
            foreach (var part in Parts)
                action(part, argument);
        }
    }
}
