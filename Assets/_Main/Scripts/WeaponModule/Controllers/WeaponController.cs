using System;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using InputModule;
using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponController : IWeapon, IInitializable, ITickable, ILateTickable, IDisposable, IPausable
    {
        private readonly WeaponConfig _config;
        private readonly WeaponModel _model;
        private readonly WeaponView _view;
        private readonly IInputMap _input;
        private readonly AmmoReserveService _ammoReserveService;
        private readonly IPauseManager _pauseManager;

        private float _nextFireTime;
        private bool _isReloading;
        private bool _isAiming;
        private bool _isAimToggleActive;
        private bool _shouldShootFrame;
        private bool _isPaused;

        private CancellationTokenSource _tokenSource;

        public WeaponController(
            WeaponConfig config,
            WeaponModel model,
            WeaponView view,
            IInputMap input,
            AmmoReserveService ammoReserveService,
            IPauseManager pauseManager)
        {
            _config = config;
            _model = model;
            _view = view;
            _input = input;
            _ammoReserveService = ammoReserveService;
            _pauseManager = pauseManager;

            _tokenSource = new();
        }

        public WeaponModel Model => _model;

        public void Initialize()
        {
            _pauseManager.Register(this);

            _ammoReserveService.GetAmmoState(
                _config.name,
                _config.MagazineCapacity,
                _config.InitialReserveAmmo,
                out int currentMagazineAmmo,
                out int reserveAmmo);

            _model.Initialize(_config.MagazineCapacity, currentMagazineAmmo, reserveAmmo);
            _view.Initialize(_config);
        }

        public void Equip()
        {
            CancelCurrentActions();
            _view.gameObject.SetActive(true);
            _isReloading = false;
            _isAiming = false;
            _isAimToggleActive = false;
            _shouldShootFrame = false;

            DrawWeaponRoutine().Forget();
        }

        public async UniTask Unequip()
        {
            SaveAmmoState();
            CancelCurrentActions();

            _view.SetHolsterState(true);

            bool canceled = await WaitWhilePlayableAsync(_config.DrawTime, _tokenSource.Token);

            if (!canceled)
                _view.gameObject.SetActive(false);
        }

        public void Hide()
        {
            CancelCurrentActions();
            _view.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            _pauseManager.Unregister(this);
            SaveAmmoState();
            CancelCurrentActions();
            _tokenSource.Dispose();
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            _shouldShootFrame = false;
            _view.SetPaused(isPaused);
        }

        public void Tick()
        {
            if (_isPaused || !_view.gameObject.activeSelf)
                return;

            HandleAimingState();
            CheckFireInput();
            HandleReload();
            HandleTriggerFinger();
            HandleMovementAnim();
        }

        public void LateTick()
        {
            if (_isPaused || !_view.gameObject.activeSelf)
                return;

            HandleProceduralAnimation();

            if (_shouldShootFrame)
            {
                Fire();
                _shouldShootFrame = false;
            }
        }

        private void HandleProceduralAnimation()
        {
            Vector2 lookDelta = _input.LookInput;
            _view.UpdateProcedural(Time.deltaTime, lookDelta, _isAiming);
        }

        private void HandleAimingState()
        {
            if (_isReloading)
            {
                _isAiming = false;
                _isAimToggleActive = false;
            }
            else
            {
                if (_config.AimIsToggle)
                {
                    if (_input.IsAimTriggered)
                        _isAimToggleActive = !_isAimToggleActive;

                    _isAiming = _isAimToggleActive;
                }
                else
                {
                    _isAiming = _input.IsAimPressed;
                }
            }

            _view.UpdateAiming(_isAiming, _config.AimStability);
        }

        private void CheckFireInput()
        {
            if (_isReloading)
                return;

            if (_input.IsFireHeld && Time.time >= _nextFireTime)
            {
                if (_model.CurrentMagazineAmmo > 0)
                {
                    _shouldShootFrame = true;
                    _nextFireTime = Time.time + _config.FireRate;
                }
            }
        }

        private void Fire()
        {
            if (!_model.TryConsumeAmmo())
                return;

            SaveAmmoState();

            _view.PlayFireEffects(_isAiming);
            _view.SpawnBullet(_config.Damage, _config.BulletSpeed);

            if (_model.IsMagazineEmpty && _model.CanReload)
                ReloadRoutine(true, _config.EmptyReloadDelay).Forget();
        }

        private void HandleReload()
        {
            if (_isReloading)
                return;

            if (_input.IsReloadPressed && _model.CanReload)
                ReloadRoutine(_model.IsMagazineEmpty, 0f).Forget();
        }

        private void HandleMovementAnim()
        {
            Vector2 input = _input.MoveInput;
            bool isMoving = input.magnitude > 0.1f;
            _view.SetMovementState(isMoving, input);
        }

        private void HandleTriggerFinger()
        {
            _view.SetTriggerHold(_input.IsFireHeld);
        }

        private async UniTaskVoid ReloadRoutine(bool isEmptyReload, float delayBeforeReload)
        {
            if (_isReloading)
                return;

            if (!_model.CanReload)
                return;

            _isReloading = true;
            _shouldShootFrame = false;

            if (delayBeforeReload > 0f)
            {
                bool delayCanceled = await WaitWhilePlayableAsync(delayBeforeReload, _tokenSource.Token);

                if (delayCanceled)
                {
                    _isReloading = false;
                    return;
                }

                if (!_model.CanReload)
                {
                    _isReloading = false;
                    return;
                }
            }

            _view.PlayReload(isEmptyReload);

            bool reloadCanceled = await WaitWhilePlayableAsync(_config.ReloadTime, _tokenSource.Token);

            if (!reloadCanceled)
            {
                _model.Reload();
                SaveAmmoState();
            }

            _isReloading = false;
        }

        private async UniTaskVoid DrawWeaponRoutine()
        {
            _view.SetHolsterState(false);
            _isReloading = true;

            bool canceled = await WaitWhilePlayableAsync(_config.DrawTime, _tokenSource.Token);

            if (!canceled)
                _isReloading = false;
        }

        private async UniTask<bool> WaitWhilePlayableAsync(float duration, CancellationToken token)
        {
            var remainingTime = Mathf.Max(0f, duration);

            while (remainingTime > 0f)
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token)
                    .SuppressCancellationThrow();

                if (canceled)
                    return true;

                if (_isPaused)
                    continue;

                remainingTime -= Time.deltaTime;
            }

            return false;
        }

        private void SaveAmmoState()
        {
            _ammoReserveService.SetAmmoState(
                _config.name,
                _model.CurrentMagazineAmmo,
                _model.ReserveAmmo);
        }

        private void CancelCurrentActions()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new();
            _isReloading = false;
        }
    }
}