using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponView : MonoBehaviour
    {
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
        private Vector3 _aimPositionOffset;
        private WeaponConfig _config;
        private float _currentAimBlend;
        private Transform _cameraTransform;
        private bool _isPaused;

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

        public void SetModifications(IReadOnlyList<WeaponModificationConfig> modifications)
        {
            _aimPositionOffset = Vector3.zero;

            foreach (var part in Parts)
            {
                part.OnModificationsChanged(modifications);
                _aimPositionOffset += part.AimPositionOffset;
            }
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            SetAnimatorSpeed(isPaused ? 0f : 1f);
            ForEachPart(static (part, paused) => part.OnPaused(paused), isPaused);
        }

        public void SetTriggerHold(bool isHeld)
        {
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

        public void PlayReload(bool isEmptyReload)
        {
            ForEachPart(static (part, isEmpty) => part.OnReloadStarted(isEmpty), isEmptyReload);

            if (_config.ReloadMode == ReloadMode.SingleRound)
            {
                SetFloat(WeaponAnimatorHashes.ReloadSpeed, GetReloadSpeed(roundReloadClip, _config.RoundReloadTime));
                SetBool(WeaponAnimatorHashes.IsRoundReloading, true);
                return;
            }

            var isEmptyAnimation = isEmptyReload && _config.UseEmptyReloadAnimation;
            var clip = isEmptyAnimation ? emptyReloadClip : reloadClip;

            SetFloat(WeaponAnimatorHashes.ReloadSpeed, GetReloadSpeed(clip, _config.ReloadTime));
            SetTrigger(isEmptyAnimation ? WeaponAnimatorHashes.EmptyReload : WeaponAnimatorHashes.Reload);
        }

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

            var targetPosition = isAiming ? _config.AimPosition + _aimPositionOffset : _hipPosition;
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

            _weaponRecoil.Update(deltaTime);
            _cameraRecoil.Update(deltaTime);
            _sway.Update(lookInput, _config.Sway, deltaTime, isAiming);

            if (recoilPivot != null)
            {
                recoilPivot.localPosition = _weaponRecoil.CurrentPosition + _sway.OutputPosition;
                recoilPivot.localRotation = Quaternion.Euler(_weaponRecoil.CurrentRotation) * _sway.OutputRotation;
            }

            if (_cameraTransform != null)
                _cameraTransform.localRotation = Quaternion.Euler(_cameraRecoil.CurrentRotation);
        }

        public void ResetVisuals()
        {
            _weaponRecoil.Reset();
            _sway.Reset();
            _currentAimBlend = 0f;

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

        private void OnAnimationEventRaised(string eventId) =>
            ForEachPart(static (part, id) => part.OnAnimationEvent(id), eventId);

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
