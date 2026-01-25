using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using InputModule.Core;
using WeaponModule.Configs;
using WeaponModule.Core;
using WeaponModule.View;

namespace WeaponModule.Controllers
{
    public class WeaponController : IWeapon, IInitializable, ITickable, ILateTickable, IDisposable
    {
        private readonly WeaponConfig _config;
        private readonly WeaponModel _model;
        private readonly WeaponView _view;
        private readonly IInputMap _input;

        // Состояния
        private float _nextFireTime;
        private bool _isReloading;
        private bool _isAiming;
        private bool _isAimToggleActive; // Для режима переключения прицела

        // Флаг для синхронизации выстрела с физикой
        private bool _shouldShootFrame;

        private CancellationTokenSource _cts;

        public WeaponController(
            WeaponConfig config,
            WeaponModel model,
            WeaponView view,
            IInputMap input)
        {
            _config = config;
            _model = model;
            _view = view;
            _input = input;

            _cts = new CancellationTokenSource();
        }

        // --- IWeapon & IInitializable Implementation ---

        public void Initialize()
        {
            _model.Initialize(_config.MaxAmmo);
            _view.Initialize(_config);
        }

        public void Equip()
        {
            _view.gameObject.SetActive(true);
            _isReloading = false;
            _isAiming = false;
            _isAimToggleActive = false;
            _shouldShootFrame = false;

            DrawWeaponRoutine().Forget();
        }

        public async UniTask Unequip()
        {
            CancelCurrentActions();

            // Анимация убирания
            _view.SetHolsterState(true);

            // Ждем завершения анимации
            await UniTask.Delay(TimeSpan.FromSeconds(_config.DrawTime));

            _view.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            CancelCurrentActions();
            _cts.Dispose();
        }

        // --- TICK: Логика и Ввод ---
        public void Tick()
        {
            if (!_view.gameObject.activeSelf) return;

            // 1. Считываем состояние прицеливания
            HandleAimingState();

            // 2. Проверяем, надо ли стрелять (но не спавним пулю тут!)
            CheckFireInput();

            // 3. Обрабатываем нажатие R
            HandleReload();

            // 4. Анимации пальцев и ходьбы
            HandleTriggerFinger();
            HandleMovementAnim();
        }

        // --- LATE TICK: Визуал и Спавн ---
        // Вызывается после всех Update, чтобы синхронизировать отдачу и вылет пули
        public void LateTick()
        {
            if (!_view.gameObject.activeSelf) return;

            // 1. Применяем отдачу и покачивание (Двигаем ствол)
            HandleProceduralAnimation();

            // 2. Если в Tick мы решили стрелять - стреляем сейчас (из сдвинутого ствола)
            if (_shouldShootFrame)
            {
                Fire();
                _shouldShootFrame = false;
            }
        }

        // --- Logic Methods ---

        private void HandleProceduralAnimation()
        {
            Vector2 lookDelta = _input.LookInput;
            // Обновляем позицию RecoilPivot
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
                    if (_input.IsAimTriggered) _isAimToggleActive = !_isAimToggleActive;
                    _isAiming = _isAimToggleActive;
                }
                else
                {
                    _isAiming = _input.IsAimPressed;
                }
            }

            // Передаем во View состояние и параметр стабильности для аниматора
            _view.UpdateAiming(_isAiming, _config.AimStability);
        }

        private void CheckFireInput()
        {
            if (_isReloading) return;

            if (_input.IsFirePressed && Time.time >= _nextFireTime)
            {
                if (_model.CurrentAmmo > 0)
                {
                    // Ставим флаг, чтобы выстрелить в LateTick
                    _shouldShootFrame = true;
                    // Обновляем таймер сразу, чтобы не стрелять дважды
                    _nextFireTime = Time.time + _config.FireRate;
                }
                else
                {
                    // Тут можно добавить звук "Dry Fire" (Клик)
                }
            }
        }

        private void Fire()
        {
            // Пытаемся забрать патрон
            if (_model.TryConsumeAmmo())
            {
                // 1. Визуальные эффекты (Вспышка, Звук, Анимация)
                // Передаем _isAiming, чтобы View знала, какую отдачу применять
                _view.PlayFireEffects(_isAiming);

                // 2. Спавн пули
                _view.SpawnBullet(_config.Damage, _config.BulletSpeed);
            }
        }

        private void HandleReload()
        {
            if (_isReloading) return;

            if (_input.IsReloadPressed && !_model.IsFull)
            {
                ReloadRoutine().Forget();
            }
        }

        private void HandleMovementAnim()
        {
            Vector2 input = _input.MoveInput;
            bool isMoving = input.magnitude > 0.1f;
            _view.SetMovementState(isMoving, input);
        }

        private void HandleTriggerFinger()
        {
            _view.SetTriggerHold(_input.IsFirePressed);
        }

        // --- Async Routines ---

        private async UniTaskVoid ReloadRoutine()
        {
            _isReloading = true;
            _view.PlayReload();

            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(_config.ReloadTime), cancellationToken: _cts.Token).SuppressCancellationThrow();

            if (!canceled)
            {
                _model.Reload();
            }

            _isReloading = false;
        }

        private async UniTaskVoid DrawWeaponRoutine()
        {
            _view.SetHolsterState(false); // Draw Trigger

            // Блокируем стрельбу на время доставания
            _isReloading = true;

            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(_config.DrawTime), cancellationToken: _cts.Token).SuppressCancellationThrow();

            if (!canceled) _isReloading = false;
        }

        private void CancelCurrentActions()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _isReloading = false;
        }

        public class Factory : PlaceholderFactory<WeaponConfig, WeaponView, WeaponController> { }
    }
}