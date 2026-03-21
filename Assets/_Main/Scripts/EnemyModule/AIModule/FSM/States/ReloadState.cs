using ComponentsModule;
using FSMModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class ReloadState : IState
    {
        private readonly EnemyConfig _config;

        private readonly Blackboard _blackboard;

        private readonly IRangedAttackComponent _attack;
        private readonly IPathMoveComponent _movement;
        private readonly ICoverFinderComponent _coverFinder;

        private float _reloadTimer;
        private bool _isReloadComplete;

        private bool _movingToCover;

        public ReloadState(EnemyConfig config,
            IRangedAttackComponent attack,
            Blackboard blackboard,
            IPathMoveComponent movement,
            ICoverFinderComponent coverFinder)
        {
            _config = config;
            _attack = attack;
            _blackboard = blackboard;
            _movement = movement;
            _coverFinder = coverFinder;
        }

        public bool IsReloadComplete => _isReloadComplete;

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

            _attack.Reload();
            _isReloadComplete = true;
        }

        public void OnExit()
        {
            _movingToCover = false;
            _isReloadComplete = false;
            _movement.Stop();
        }

        private bool TryMoveToCover()
        {
            if (!_blackboard.TryGetValue(BlackboardTag.TargetPosition, out Vector3 targetPosition))
                return false;

            var foundCover = _coverFinder.TryFindCover(targetPosition,
                _config.CoverSearchRadius,
                _config.ViewMask,
                out var coverPosition);

            if (!foundCover)
                return false;

            _movement.MoveTo(coverPosition);
            return true;
        }
    }
}