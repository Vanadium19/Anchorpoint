using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace NpcBaseModule
{
    public sealed class FriendlyNpcController : ITickable, IInitializable
    {
        private const float MinRemainingDistance = 0.1f;
        private const float RetryWaitSeconds = 0.5f;
        private const int NavMeshSampleAttempts = 30;
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly FriendlyNpcView _view;

        private FriendlyNpcState _state;
        private Transform _player;

        private float _waitTimer;
        private Vector3 _currentDestination;
        private bool _hasDestination;

        private int _relationshipLevel;
        private int _currentDialogueLineIndex;
        private const string SpeedParameterName = "Speed";

        public FriendlyNpcController(FriendlyNpcView view)
        {
            _view = view;
            _state = FriendlyNpcState.Patrolling;
        }

        public void Initialize()
        {
            _view.SetController(this);

            _relationshipLevel = _view.Settings.StartRelationshipLevel;

            ApplyAgentSettings();
            HideText();
            _view.DialoguePresenter?.Close();

            StartPatrol(forceNewPoint: true);
        }

        public void OnInteract(Transform player)
        {
            if (player == null)
                return;

            if (_state == FriendlyNpcState.Responding)
                return;

            _player = player;
            EnterResponding();
        }

        public void Tick()
        {
            if (_view.Agent == null)
                return;

            UpdateAnimator();

            switch (_state)
            {
                case FriendlyNpcState.Patrolling:
                    TickPatrolling();
                    return;

                case FriendlyNpcState.Responding:
                    TickResponding();
                    return;

                default:
                    return;
            }
        }

        private void UpdateAnimator()
        {
            Animator animator = _view.Animator;

            if (animator == null)
                return;

            NavMeshAgent agent = _view.Agent;
            float speed = 0f;

            if (_state == FriendlyNpcState.Patrolling && !agent.isStopped && _hasDestination)
            {
                float remainingDistanceThreshold = Mathf.Max(agent.stoppingDistance, MinRemainingDistance);

                if (!agent.pathPending && agent.remainingDistance > remainingDistanceThreshold)
                    speed = agent.velocity.magnitude;
            }

            if (speed < 0.05f)
                speed = 0f;

            animator.SetFloat(SpeedParameterName, speed);
        }

        private void TickPatrolling()
        {
            var agent = _view.Agent;

            if (!_hasDestination)
            {
                if (_waitTimer > 0f)
                {
                    _waitTimer -= Time.deltaTime;
                    return;
                }

                TrySetNewDestination();
                return;
            }

            if (agent.pathPending)
                return;

            var remainingDistanceThreshold = Mathf.Max(agent.stoppingDistance, MinRemainingDistance);

            if (agent.remainingDistance <= remainingDistanceThreshold)
            {
                _hasDestination = false;
                _waitTimer = Random.Range(_view.Settings.MinWait, _view.Settings.MaxWait);
            }
        }

        private void TickResponding()
        {
            if (_player == null)
            {
                ExitResponding();
                return;
            }

            var distance = Vector3.Distance(_player.position, _view.Transform.position);

            if (distance > _view.Settings.ExitRadius)
            {
                ExitResponding();
                return;
            }

            FaceTarget(_player.position);
        }

        private void EnterResponding()
        {
            _state = FriendlyNpcState.Responding;

            var agent = _view.Agent;

            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            HideText();

            _currentDialogueLineIndex = 0;
            _view.DialoguePresenter?.Begin(_view, OnDialogueButtonSelected);

            RenderCurrentDialogueLine();
        }

        private void ExitResponding()
        {
            _player = null;
            HideText();
            _view.DialoguePresenter?.Close();

            var agent = _view.Agent;

            if (agent != null)
                agent.isStopped = false;

            _state = FriendlyNpcState.Patrolling;
            StartPatrol(forceNewPoint: true);
        }

        private void OnDialogueButtonSelected(int answerIndex)
        {
            var line = GetCurrentDialogueLine();

            if (line == null)
            {
                ExitResponding();
                return;
            }

            if (line.HasAnswers)
            {
                if (answerIndex < 0 || answerIndex >= line.Answers.Count)
                    return;

                var answer = line.Answers[answerIndex];
                _relationshipLevel += answer.RelationshipDelta;
                _currentDialogueLineIndex = answer.NextLineIndex;
            }
            else
            {
                _currentDialogueLineIndex = line.NextLineIndex;
            }

            if (!IsValidDialogueLineIndex(_currentDialogueLineIndex))
            {
                ExitResponding();
                return;
            }

            RenderCurrentDialogueLine();
        }

        private void RenderCurrentDialogueLine()
        {
            var presenter = _view.DialoguePresenter;

            if (presenter == null)
            {
                ShowText();
                return;
            }

            var line = GetCurrentDialogueLine();

            if (line == null)
            {
                presenter.Render(
                    _view.Settings.NpcName,
                    _relationshipLevel,
                    _view.Settings.MessageText,
                    System.Array.Empty<NpcDialogueAnswer>(),
                    hasNextLine: false
                );

                return;
            }

            var hasNextLine = !line.HasAnswers && IsValidDialogueLineIndex(line.NextLineIndex);

            presenter.Render(
                _view.Settings.NpcName,
                _relationshipLevel,
                line.Text,
                line.Answers,
                hasNextLine
            );
        }

        private NpcDialogueLine GetCurrentDialogueLine()
        {
            if (!IsValidDialogueLineIndex(_currentDialogueLineIndex))
                return null;

            return _view.Settings.DialogueLines[_currentDialogueLineIndex];
        }

        private bool IsValidDialogueLineIndex(int lineIndex)
        {
            var lines = _view.Settings.DialogueLines;

            return lines != null && lineIndex >= 0 && lineIndex < lines.Count;
        }

        private void StartPatrol(bool forceNewPoint)
        {
            _hasDestination = false;
            _waitTimer = 0f;

            if (forceNewPoint)
                TrySetNewDestination();
        }

        private void TrySetNewDestination()
        {
            var agent = _view.Agent;

            if (TryGetRandomNavMeshPoint(_view.Transform.position, _view.Settings.PatrolRadius, out var point))
            {
                _currentDestination = point;
                _hasDestination = true;
                agent.SetDestination(_currentDestination);
                return;
            }

            _hasDestination = false;
            _waitTimer = RetryWaitSeconds;
        }

        private bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
        {
            for (var attemptIndex = 0; attemptIndex < NavMeshSampleAttempts; attemptIndex++)
            {
                var randomPoint = origin + Random.insideUnitSphere * radius;

                if (NavMesh.SamplePosition(randomPoint, out var hit, radius, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = origin;
            return false;
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            var selfPosition = _view.Transform.position;
            var direction = targetPosition - selfPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                return;

            var targetYaw = Quaternion.LookRotation(direction, Vector3.up).eulerAngles.y;
            var currentYaw = _view.Transform.eulerAngles.y;

            var newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, _view.Settings.TurnSpeed * Time.deltaTime);
            _view.Transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }

        private void ApplyAgentSettings()
        {
            var agent = _view.Agent;

            if (agent == null)
                return;

            agent.speed = _view.Settings.MoveSpeed;
        }

        private void ShowText() => _view.WorldText?.Show(_view.Settings.MessageText);

        private void HideText() => _view.WorldText?.Hide();
    }
}