using Cysharp.Threading.Tasks;
using EnemyModule.Configs;
using EnemyModule.Core;
using EnemyModule.View;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace EnemyModule.Controllers
{
    public class EnemyController : IInitializable, ITickable, IDisposable
    {
        private readonly EnemyConfig _config;
        private readonly EnemyModel _model;
        private readonly EnemyView _view;
        private CancellationTokenSource _cts;

        private Transform _playerTransform;
        private float _nextFireTime;
        private float _chaseTimer;
        private int _patrolIndex = 0;
        private bool _isActionBusy;

        public EnemyController(EnemyConfig config, EnemyModel model, EnemyView view)
        {
            _config = config;
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) _playerTransform = playerObj.transform;

            _view.Initialize(_config);
            _model.Initialize(_config.MaxAmmo, _view.transform.position);

            _view.Health.DamageTaken += OnTakeDamage;
            _view.Health.Died += OnDeath;
        }

        public void Dispose()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            if (_view != null && _view.Health != null)
            {
                _view.Health.DamageTaken -= OnTakeDamage;
                _view.Health.Died -= OnDeath;
            }
        }

        public void Tick()
        {
            if (_view == null || _view.Health == null || !_view.Health.IsAlive) return;
            _view.UpdateAnimator(_view.Velocity);
            if (_isActionBusy) return;
            _view.UpdateAnimator(_view.Velocity);
            switch (_model.CurrentState)
            {
                case EnemyModel.AIState.Patrol:
                    UpdatePatrol();
                    break;
                case EnemyModel.AIState.Chase:
                    UpdateChase();
                    break;
                case EnemyModel.AIState.Attack:
                    UpdateAttack();
                    break;
                case EnemyModel.AIState.Reload:
                    break;
            }
        }

        private void UpdatePatrol()
        {
            if (TrySpotPlayer()) return;

            if (_view.PatrolPoints.Length == 0) return;

            if (!_view.IsPathPending && _view.RemainingDistance < 0.5f)
            {
                PatrolWaitRoutine().Forget();
            }
        }

        private async UniTaskVoid PatrolWaitRoutine()
        {
            _isActionBusy = true;
            _view.StopMove();

            float totalWaitTime = _config.PatrolWaitTime;
            float timer = 0;
            float nextLookTime = 0;

            while (timer < totalWaitTime)
            {
                float dt = Time.deltaTime;
                timer += dt;

                if (TrySpotPlayer())
                {
                    _isActionBusy = false;
                    return;
                }

                if (timer >= nextLookTime)
                {
                    nextLookTime = timer + UnityEngine.Random.Range(1.5f, 3f);

                    float randomAngle = UnityEngine.Random.Range(-_config.LookAngleRange, _config.LookAngleRange);

                    RotateToAngleRoutine(randomAngle).Forget();
                }
                await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token).SuppressCancellationThrow();
                if (_view == null || !_view.Health.IsAlive) return;
            }

            _patrolIndex = (_patrolIndex + 1) % _view.PatrolPoints.Length;
            _view.MoveTo(_view.PatrolPoints[_patrolIndex].position);

            _isActionBusy = false;
        }
        private async UniTaskVoid RotateToAngleRoutine(float angleOffset)
        {
            Quaternion startRot = _view.transform.rotation;
            Quaternion targetRot = startRot * Quaternion.Euler(0, angleOffset, 0);

            float t = 0;
            float duration = 1.0f;

            while (t < 1f)
            {
                if (_cts.IsCancellationRequested || !_view.Health.IsAlive) return;

                if (_model.CurrentState != EnemyModel.AIState.Patrol) return;

                t += Time.deltaTime / duration;

                _view.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

                await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token).SuppressCancellationThrow();
            }
        }
        private void UpdateChase()
        {
            float dist = Vector3.Distance(_view.transform.position, _model.LastKnownPosition);
            bool canSee = CheckVision(out Vector3 targetPos);

            if (canSee)
            {
                _model.SetTargetPosition(targetPos);
                _chaseTimer = _config.MemoryTime;

                if (dist <= _config.AttackRange)
                {
                    _model.SetState(EnemyModel.AIState.Attack);
                    _view.StopMove();
                }
                else
                {
                    _view.MoveTo(_model.LastKnownPosition);
                }
            }
            else
            {
                _view.MoveTo(_model.LastKnownPosition);

                if (dist < 2f)
                {
                    _chaseTimer -= Time.deltaTime;
                    if (_chaseTimer <= 0)
                    {
                        _model.SetState(EnemyModel.AIState.Patrol);
                        _view.MoveTo(_view.PatrolPoints[_patrolIndex].position);
                    }
                }
            }
        }

        private void UpdateAttack()
        {
            if (_model.CurrentAmmo <= 0)
            {
                ReloadRoutine().Forget();
                return;
            }

            bool canSee = CheckVision(out Vector3 targetPos);
            float dist = Vector3.Distance(_view.transform.position, _model.LastKnownPosition);

            if (!canSee || dist > _config.AttackRange * 1.2f)
            {
                _model.SetState(EnemyModel.AIState.Chase);
                return;
            }

            _model.SetTargetPosition(targetPos);
            _view.StopMove();
            _view.RotateTowards(_model.LastKnownPosition);

            if (Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + 1f / _config.FireRate;
                if (_model.TryConsumeAmmo())
                {
                    _view.SpawnBullet(targetPos);
                }
            }
        }

        private async UniTaskVoid ReloadRoutine()
        {
            _model.SetState(EnemyModel.AIState.Reload);
            _isActionBusy = true;

            if (_view.FindCover(_model.LastKnownPosition, 15f, out Vector3 coverPos))
            {
                _view.MoveTo(coverPos);
                while (_view != null && _view.RemainingDistance > 1f)
                {
                    if (_cts.IsCancellationRequested) return;
                    await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token).SuppressCancellationThrow();
                }
            }

            if (_view == null || _cts.IsCancellationRequested) return;

            _view.StopMove();

            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(_config.ReloadTime), cancellationToken: _cts.Token).SuppressCancellationThrow();

            if (canceled) return;
            if (_view == null) return;

            _model.Reload(_config.MaxAmmo);
            _model.SetState(EnemyModel.AIState.Chase);
            _isActionBusy = false;
        }

        private bool CheckVision(out Vector3 targetPos)
        {
            return _view.CheckLineOfSight(_playerTransform, _config.SightDistance, _config.ViewAngle, _config.ViewMask, out targetPos);
        }

        private bool TrySpotPlayer()
        {
            if (_playerTransform == null) return false;
            if (CheckVision(out Vector3 pos))
            {
                _model.SetTargetPosition(pos);
                _model.SetState(EnemyModel.AIState.Chase);
                return true;
            }
            return false;
        }

        private void OnTakeDamage(Vector3? hitPos, Vector3? force)
        {
            if (_playerTransform)
            {
                _model.SetTargetPosition(_playerTransform.position);
                _model.SetState(EnemyModel.AIState.Chase);
            }
        }

        private void OnDeath()
        {
            _view.Die(null, null);
        }
    }
}