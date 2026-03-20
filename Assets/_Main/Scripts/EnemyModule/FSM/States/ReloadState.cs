using ComponentsModule;
using FSMModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class ReloadState : IState
    {
        private readonly EnemyConfig _config;
        private readonly Enemy _enemy;
        private readonly IPathMoveComponent _movement;
        private readonly ICoverFinderComponent _coverFinder;

        private float _reloadTimer;
        private bool _movingToCover;
        private bool _isReloadComplete;

        public ReloadState(
            EnemyConfig config,
            Enemy enemy,
            IPathMoveComponent movement,
            ICoverFinderComponent coverFinder)
        {
            _config = config;
            _enemy = enemy;
            _movement = movement;
            _coverFinder = coverFinder;
        }

        public void OnEnter()
        {
            _reloadTimer = _config.ReloadTime;
            _movingToCover = TryMoveToCover();
            _isReloadComplete = false;

            if (!_movingToCover)
                _movement.Stop();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_movingToCover)
            {
                if (_movement.IsPathPending || _movement.RemainingDistance > _config.CoverArrivalDistance)
                    return;

                _movingToCover = false;
                _movement.Stop();
            }

            _reloadTimer -= deltaTime;

            if (_reloadTimer > 0f)
                return;

            _enemy.Reload(_config.MaxAmmo);
            _isReloadComplete = true;
        }

        public void OnExit()
        {
            _movingToCover = false;
            _isReloadComplete = false;
            _movement.Stop();
        }

        public bool IsReloadComplete => _isReloadComplete;

        private bool TryMoveToCover()
        {
            bool foundCover = _coverFinder.TryFindCover(
                _enemy.LastKnownPosition,
                _config.CoverSearchRadius,
                _config.ViewMask,
                out Vector3 coverPosition);

            if (!foundCover)
                return false;

            _movement.MoveTo(coverPosition);
            return true;
        }
    }
}
