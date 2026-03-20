using System;
using System.Threading;
using ComponentsModule;
using Cysharp.Threading.Tasks;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class AIAgent : IDisposable
    {
        private const float PatrolLookDuration = 1f;

        private readonly EnemyConfig _config;
        private readonly Enemy _model;
        private readonly EnemyView _view;
        private readonly PlayerProvider _player;
        private readonly IHealthComponent _health;
        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;
        private readonly ICoverFinderComponent _coverFinder;
        private readonly Transform[] _patrolPoints;

        private CancellationTokenSource _cancellationToken;
        private Transform _playerTransform;
        private float _nextFireTime;
        private float _chaseTimer;
        private int _patrolIndex;
        private bool _isActionBusy;

        public AIAgent(EnemyConfig config,
            Enemy model,
            EnemyView view,
            PlayerProvider player,
            IHealthComponent health,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight,
            ICoverFinderComponent coverFinder,
            Transform[] patrolPoints)
        {
            _config = config;
            _model = model;
            _view = view;
            _player = player;
            _health = health;
            _movement = movement;
            _rotation = rotation;
            _lineOfSight = lineOfSight;
            _coverFinder = coverFinder;
            _patrolPoints = patrolPoints;
        }

        public void Initialize()
        {
            DisposeCancellation();

            _cancellationToken = new CancellationTokenSource();
            _nextFireTime = 0f;
            _chaseTimer = 0f;
            _patrolIndex = 0;
            _isActionBusy = false;
            _playerTransform = null;

            if (_player.TryGet(out Transform playerTransform))
                _playerTransform = playerTransform;

            _movement.SetStoppingDistance(_config.StoppingDistance);

            if (_patrolPoints.Length > 0)
                _movement.MoveTo(_patrolPoints[_patrolIndex].position);
        }

        public void Tick()
        {
            if (!_health.IsAlive || _isActionBusy)
                return;

            switch (_model.CurrentState)
            {
                case AIState.Patrol:
                    UpdatePatrol();
                    return;

                case AIState.Chase:
                    UpdateChase();
                    return;

                case AIState.Attack:
                    UpdateAttack();
                    return;

                case AIState.Reload:
                default:
                    return;
            }
        }

        public void OnTakeDamage(Vector3? hitPos, Vector3? force)
        {
            if (_playerTransform == null)
                return;

            _model.SetTargetPosition(_playerTransform.position);
            _model.SetState(AIState.Chase);
        }

        public void Dispose()
        {
            DisposeCancellation();
        }

        private void UpdatePatrol()
        {
            if (TrySpotPlayer())
                return;

            if (_patrolPoints.Length == 0)
                return;

            if (_movement.IsPathPending)
                return;

            if (_movement.RemainingDistance > _config.StoppingDistance)
                return;

            PatrolWaitRoutine().Forget();
        }

        private async UniTaskVoid PatrolWaitRoutine()
        {
            CancellationToken token = _cancellationToken.Token;

            _isActionBusy = true;
            _movement.Stop();

            try
            {
                float elapsedTime = 0f;
                float nextLookTime = 0f;

                while (elapsedTime < _config.PatrolWaitTime)
                {
                    if (_model.CurrentState != AIState.Patrol || !_health.IsAlive)
                        return;

                    if (TrySpotPlayer())
                        return;

                    if (elapsedTime >= nextLookTime)
                    {
                        nextLookTime = elapsedTime + Mathf.Max(_config.LookInterval, 0.1f);

                        float randomAngle = UnityEngine.Random.Range(-_config.LookAngleRange, _config.LookAngleRange);
                        RotateToAngleRoutine(randomAngle).Forget();
                    }

                    elapsedTime += Time.deltaTime;

                    await UniTask.Yield(PlayerLoopTiming.Update, token)
                        .SuppressCancellationThrow();

                    if (token.IsCancellationRequested)
                        return;
                }

                MoveToNextPatrolPoint();
            }
            finally
            {
                _isActionBusy = false;
            }
        }

        private async UniTaskVoid RotateToAngleRoutine(float angleOffset)
        {
            CancellationToken token = _cancellationToken.Token;
            Quaternion targetRotation = _view.transform.rotation * Quaternion.Euler(0f, angleOffset, 0f);
            float turnSpeed = Quaternion.Angle(_view.transform.rotation, targetRotation) / PatrolLookDuration;

            while (Quaternion.Angle(_view.transform.rotation, targetRotation) > 0.1f)
            {
                if (token.IsCancellationRequested || !_health.IsAlive)
                    return;

                if (_model.CurrentState != AIState.Patrol)
                    return;

                _rotation.RotateTo(targetRotation, turnSpeed);

                await UniTask.Yield(PlayerLoopTiming.Update, token)
                    .SuppressCancellationThrow();

                if (token.IsCancellationRequested)
                    return;
            }
        }

        private void UpdateChase()
        {
            bool canSeePlayer = CheckVision(out Vector3 targetPosition);
            Vector3 destination = canSeePlayer ? targetPosition : _model.LastKnownPosition;
            float distanceToTarget = Vector3.Distance(_view.transform.position, destination);

            if (canSeePlayer)
            {
                _model.SetTargetPosition(targetPosition);
                _chaseTimer = _config.MemoryTime;

                if (distanceToTarget <= _config.AttackRange)
                {
                    _model.SetState(AIState.Attack);
                    _movement.Stop();
                    return;
                }
            }

            _movement.MoveTo(_model.LastKnownPosition);

            if (canSeePlayer)
                return;

            if (distanceToTarget > _config.LostTargetReachDistance)
                return;

            _chaseTimer -= Time.deltaTime;

            if (_chaseTimer > 0f)
                return;

            EnterPatrol();
        }

        private void UpdateAttack()
        {
            if (_model.CurrentAmmo <= 0)
            {
                ReloadRoutine().Forget();
                return;
            }

            bool canSeePlayer = CheckVision(out Vector3 targetPosition);
            Vector3 attackPoint = canSeePlayer ? targetPosition : _model.LastKnownPosition;
            float distanceToTarget = Vector3.Distance(_view.transform.position, attackPoint);

            if (!canSeePlayer || distanceToTarget > _config.AttackRange * _config.AttackExitRangeMultiplier)
            {
                _model.SetState(AIState.Chase);
                return;
            }

            _model.SetTargetPosition(targetPosition);
            _movement.Stop();
            _rotation.RotateTowards(_model.LastKnownPosition, _config.CombatTurnSpeed);

            if (Time.time < _nextFireTime)
                return;

            float fireRate = Mathf.Max(_config.FireRate, 0.01f);
            _nextFireTime = Time.time + (1f / fireRate);

            if (_model.TryConsumeAmmo())
                _view.SpawnBullet(targetPosition, _config.Damage, _config.BulletSpeed);
        }

        private async UniTaskVoid ReloadRoutine()
        {
            CancellationToken token = _cancellationToken.Token;

            _model.SetState(AIState.Reload);
            _isActionBusy = true;

            try
            {
                if (_coverFinder.TryFindCover(_model.LastKnownPosition, _config.CoverSearchRadius, _config.ViewMask, out Vector3 coverPosition))
                {
                    _movement.MoveTo(coverPosition);

                    while (!token.IsCancellationRequested && _health.IsAlive)
                    {
                        if (_movement.RemainingDistance <= _config.CoverArrivalDistance)
                            break;

                        await UniTask.Yield(PlayerLoopTiming.Update, token)
                            .SuppressCancellationThrow();

                        if (token.IsCancellationRequested)
                            return;
                    }
                }

                if (token.IsCancellationRequested || !_health.IsAlive)
                    return;

                _movement.Stop();

                bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(_config.ReloadTime),
                        cancellationToken: token)
                    .SuppressCancellationThrow();

                if (canceled || token.IsCancellationRequested || !_health.IsAlive)
                    return;

                _model.Reload(_config.MaxAmmo);
                _model.SetState(AIState.Chase);
            }
            finally
            {
                _isActionBusy = false;
            }
        }

        private bool CheckVision(out Vector3 targetPosition)
        {
            targetPosition = default;

            if (_playerTransform == null)
                return false;

            return _lineOfSight.CheckLineOfSight(_playerTransform,
                _config.SightDistance,
                _config.ViewAngle,
                _config.ViewMask,
                out targetPosition);
        }

        private bool TrySpotPlayer()
        {
            if (!CheckVision(out Vector3 playerPosition))
                return false;

            _model.SetTargetPosition(playerPosition);
            _model.SetState(AIState.Chase);
            _chaseTimer = _config.MemoryTime;
            return true;
        }

        private void EnterPatrol()
        {
            _model.SetState(AIState.Patrol);

            if (_patrolPoints.Length == 0)
            {
                _movement.Stop();
                return;
            }

            _movement.MoveTo(_patrolPoints[_patrolIndex].position);
        }

        private void MoveToNextPatrolPoint()
        {
            if (_patrolPoints.Length == 0)
                return;

            _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
            _movement.MoveTo(_patrolPoints[_patrolIndex].position);
        }

        private void DisposeCancellation()
        {
            if (_cancellationToken == null)
                return;

            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
            _cancellationToken = null;
        }
    }
}