using System;
using System.Collections.Generic;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponController : IWeapon, IInitializable, ITickable, ILateTickable, IDisposable, IPausable
    {
        private readonly WeaponConfig _config;
        private readonly WeaponModel _model;
        private readonly WeaponView _view;
        private readonly IWeaponStatsProvider _statsProvider;
        private readonly IWeaponViewFactory _viewFactory;
        private readonly IBulletFactory _bulletFactory;
        private readonly IPauseManager _pauseManager;
        private readonly Transform _ownerRoot;
        private readonly List<Bullet> _shotProjectiles = new();

        private float _nextFireTime;
        private bool _isReloading;
        private bool _isRoundReloading;
        private bool _isAiming;
        private bool _isAimToggleActive;
        private bool _shouldShootFrame;
        private bool _isPaused;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private Vector3 _playerVelocity;
        private bool _isFireHeld;
        private bool _wasFireHeld;
        private bool _isFirePressed;
        private float _pendingTriggerPullTime = float.NegativeInfinity;
        private bool _isReloadPressed;
        private bool _isAimPressed;
        private bool _isAimTriggered;

        private CancellationTokenSource _tokenSource;

        private WeaponStats _stats;
        private WeaponItemSo _weaponItem;
        private ItemTable _itemTable;

        public WeaponView GetView() => _view;

        public WeaponController(
            WeaponConfig config,
            WeaponModel model,
            WeaponView view,
            IWeaponStatsProvider statsProvider,
            IWeaponViewFactory viewFactory,
            IBulletFactory bulletFactory,
            IPauseManager pauseManager,
            Transform ownerRoot)
        {
            _config = config;
            _model = model;
            _view = view;
            _statsProvider = statsProvider;
            _viewFactory = viewFactory;
            _bulletFactory = bulletFactory;
            _pauseManager = pauseManager;
            _ownerRoot = ownerRoot;

            _stats = new WeaponStats(config, Array.Empty<WeaponModificationConfig>());
            _tokenSource = new();
            _pauseManager.Register(this);
        }

        public WeaponModel Model => _model;

        public AmmoType AmmoType => _config.AmmoType;

        public void Initialize()
        {
            _model.Initialize(_stats.MagazineCapacity, _stats.MagazineCapacity, _config.InitialReserveAmmo);
            _view.Initialize(_config);
            _view.SetLoadedRounds(_model.CurrentMagazineAmmo);
        }

        public void InitializeFromItem(WeaponItemSo weaponItem, WeaponStats stats,
            IReadOnlyList<WeaponModificationConfig> modifications, int currentMagazineAmmo, int reserveAmmo)
        {
            _weaponItem = weaponItem;
            _stats = stats;

            _model.Initialize(_stats.MagazineCapacity, currentMagazineAmmo, reserveAmmo);
            _view.Initialize(_config);
            _view.SetModifications(modifications);
            _view.SetLoadedRounds(_model.CurrentMagazineAmmo);
        }

        public void ApplyModifications(WeaponStats stats, IReadOnlyList<WeaponModificationConfig> modifications)
        {
            _stats = stats;

            _model.SetMagazineCapacity(_stats.MagazineCapacity);
            _view.SetModifications(modifications);
            _view.SetLoadedRounds(_model.CurrentMagazineAmmo);
        }

        public void SetWeaponItem(WeaponItemSo weaponItem) => _weaponItem = weaponItem;

        public WeaponItemSo GetWeaponItem() => _weaponItem;

        public void SetItemTable(ItemTable itemTable) => _itemTable = itemTable;

        public ItemTable GetItemTable() => _itemTable;

        public void SetAmmo(int magazineAmmo, int reserveAmmo) => _model.SetAmmo(magazineAmmo, reserveAmmo);

        public void SetMovementInput(Vector2 input) => _moveInput = input;

        public void SetLookInput(Vector2 input) => _lookInput = input;

        public void SetFireInput(bool isHeld, bool isPressed)
        {
            _isFireHeld = isHeld;
            _isFirePressed = isPressed;
        }

        public void SetReloadInput(bool isPressed) => _isReloadPressed = isPressed;

        public void SetPlayerVelocity(Vector3 velocity) => _playerVelocity = velocity;

        public void SetAimInput(bool isPressed, bool isTriggered)
        {
            _isAimPressed = isPressed;
            _isAimTriggered = isTriggered;
        }

        public void GetAmmoState(out int currentMagazineAmmo, out int reserveAmmo)
        {
            currentMagazineAmmo = _model.CurrentMagazineAmmo;
            reserveAmmo = _model.ReserveAmmo;
        }

        public void Equip()
        {
            CancelCurrentActions();
            _view.gameObject.SetActive(true);
            _isReloading = false;
            _isAiming = false;
            _isAimToggleActive = false;
            _shouldShootFrame = false;

            _view.ResetVisuals();
            DrawWeaponRoutine().Forget();
        }

        public async UniTask Unequip()
        {
            CancelCurrentActions();

            _view.SetHolsterState(true);

            bool canceled = await WaitWhilePlayableAsync(_config.DrawTime, _tokenSource.Token);

            if (!canceled)
                _view.gameObject.SetActive(false);
        }

        public void Hide()
        {
            CancelCurrentActions();
            _view.ResetVisuals();
            _view.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            _pauseManager.Unregister(this);
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
            _view.UpdateProcedural(Time.deltaTime, _lookInput, _isAiming);
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
                    if (_isAimTriggered)
                        _isAimToggleActive = !_isAimToggleActive;

                    _isAiming = _isAimToggleActive;
                }
                else
                {
                    _isAiming = _isAimPressed;
                }
            }

            _view.UpdateAiming(_isAiming, _config.AimStability);
        }

        private void CheckFireInput()
        {
            var isTriggerPulled = _isFirePressed || (_isFireHeld && !_wasFireHeld);

            _wasFireHeld = _isFireHeld;

            if (isTriggerPulled)
                _pendingTriggerPullTime = Time.time;

            if (_isRoundReloading && isTriggerPulled && _model.CurrentMagazineAmmo > 0)
                CancelCurrentActions();

            if (_isReloading || Time.time < _nextFireTime || _model.CurrentMagazineAmmo <= 0)
                return;

            var shouldFire = (_config.IsAutomatic && _isFireHeld) || HasPendingTriggerPull();

            if (!shouldFire)
                return;

            _pendingTriggerPullTime = float.NegativeInfinity;
            _shouldShootFrame = true;
            _nextFireTime = Time.time + _config.FireRate;
        }

        private bool HasPendingTriggerPull() => Time.time - _pendingTriggerPullTime <= _config.FireInputBuffer;

        private void Fire()
        {
            if (!_model.TryConsumeAmmo())
                return;

            _view.PlayFireEffects(_isAiming);

            _shotProjectiles.Clear();

            var deviation = UnityEngine.Random.insideUnitCircle * _stats.GetInaccuracy(_isAiming);
            var shotRotation = _view.FirePoint.rotation * Quaternion.Euler(deviation.x, deviation.y, 0f);

            for (var i = 0; i < _config.ProjectilesPerShot; i++)
            {
                var projectile = SpawnProjectile(shotRotation, i == 0);

                if (projectile != null)
                    _shotProjectiles.Add(projectile);
            }

            IgnoreCollisionsBetweenShotProjectiles();

            if (_model.IsMagazineEmpty && _model.CanReload)
                StartReload(true, _config.EmptyReloadDelay);
        }

        private Bullet SpawnProjectile(Quaternion shotRotation, bool shouldPlayHitSound)
        {
            var spread = UnityEngine.Random.insideUnitCircle * _config.SpreadAngle;
            var rotation = shotRotation * Quaternion.Euler(spread.x, spread.y, 0f);

            return _bulletFactory.SpawnBullet(_view.BulletPrefab, _view.FirePoint.position, rotation,
                _stats.Damage, _config.BulletSpeed, _config.InheritVelocity, _playerVelocity, _ownerRoot.gameObject,
                shouldPlayHitSound);
        }

        private void IgnoreCollisionsBetweenShotProjectiles()
        {
            for (var i = 0; i < _shotProjectiles.Count; i++)
            {
                for (var j = i + 1; j < _shotProjectiles.Count; j++)
                    _shotProjectiles[i].IgnoreCollisionWith(_shotProjectiles[j]);
            }

            _shotProjectiles.Clear();
        }

        private void HandleReload()
        {
            if (_isReloading)
                return;

            if (_isReloadPressed && _model.CanReload)
                StartReload(_model.IsMagazineEmpty, 0f);
        }

        private void StartReload(bool isEmptyReload, float delayBeforeReload)
        {
            if (_config.ReloadMode == ReloadMode.SingleRound)
                SingleRoundReloadRoutine(isEmptyReload, delayBeforeReload).Forget();
            else
                ReloadRoutine(isEmptyReload, delayBeforeReload).Forget();
        }

        private void HandleMovementAnim()
        {
            bool isMoving = _moveInput.magnitude > 0.1f;
            _view.SetMovementState(isMoving, _moveInput);
        }

        private void HandleTriggerFinger()
        {
            _view.SetTriggerHold(_isFireHeld);
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

            _view.SetReloadState(true);
            _view.PlayReload(isEmptyReload);
            _view.SetLoadedRounds(Mathf.Min(_model.MagazineCapacity, _model.CurrentMagazineAmmo + _model.ReserveAmmo));

            bool reloadCanceled = await WaitWhilePlayableAsync(_config.ReloadTime, _tokenSource.Token);

            if (!reloadCanceled)
                _model.Reload();

            _isReloading = false;
            _view.SetReloadState(false);
        }

        private async UniTaskVoid SingleRoundReloadRoutine(bool isEmptyReload, float delayBeforeReload)
        {
            if (_isReloading)
                return;

            if (!_model.CanReload)
                return;

            var token = _tokenSource.Token;

            _isReloading = true;
            _isRoundReloading = true;
            _shouldShootFrame = false;

            if (delayBeforeReload > 0f)
            {
                bool delayCanceled = await WaitWhilePlayableAsync(delayBeforeReload, token);

                if (delayCanceled)
                    return;

                if (!_model.CanReload)
                {
                    _isReloading = false;
                    _isRoundReloading = false;
                    return;
                }
            }

            _view.SetReloadState(true);
            _view.PlayReload(isEmptyReload);

            while (_model.CanReload)
            {
                bool roundCanceled = await WaitWhilePlayableAsync(_config.RoundReloadTime, token);

                if (roundCanceled)
                    return;

                if (_model.LoadRound())
                    _view.PlayRoundInserted();
            }

            if (isEmptyReload)
                _view.PlayActionCycle();

            _isReloading = false;
            _isRoundReloading = false;
            _view.SetReloadState(false);
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

        private void CancelCurrentActions()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new();
            _isReloading = false;
            _isRoundReloading = false;
            _view.SetReloadState(false);
            _view.SetLoadedRounds(_model.CurrentMagazineAmmo);
        }
    }
}
