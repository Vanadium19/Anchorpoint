using UnityEngine;

namespace SpawnModule
{
    public sealed class LootSpawnValidationConfig
    {
        private readonly Transform _playerSpawnPoint;
        private readonly LayerMask _floorLayer;
        private readonly LayerMask _obstacleLayer;
        private readonly float _minimumPlayerSpawnDistance;
        private readonly int _positionSearchAttempts;
        private readonly float _floorCheckHeight;
        private readonly float _floorCheckDistance;
        private readonly float _maximumFloorAngle;
        private readonly float _clearanceRadius;
        private readonly float _clearanceHeight;
        private readonly float _minimumLootSpacing;
        private readonly float _floorOffset;

        public LootSpawnValidationConfig(
            Transform playerSpawnPoint,
            LayerMask floorLayer,
            LayerMask obstacleLayer,
            float minimumPlayerSpawnDistance,
            int positionSearchAttempts,
            float floorCheckHeight,
            float floorCheckDistance,
            float maximumFloorAngle,
            float clearanceRadius,
            float clearanceHeight,
            float minimumLootSpacing,
            float floorOffset)
        {
            _playerSpawnPoint = playerSpawnPoint;
            _floorLayer = floorLayer;
            _obstacleLayer = obstacleLayer;
            _minimumPlayerSpawnDistance = Mathf.Max(minimumPlayerSpawnDistance, 0f);
            _positionSearchAttempts = Mathf.Max(positionSearchAttempts, 1);
            _floorCheckHeight = Mathf.Max(floorCheckHeight, 0f);
            _floorCheckDistance = Mathf.Max(floorCheckDistance, 0.01f);
            _maximumFloorAngle = Mathf.Clamp(maximumFloorAngle, 0f, 90f);
            _clearanceRadius = Mathf.Max(clearanceRadius, 0.01f);
            _clearanceHeight = Mathf.Max(clearanceHeight, _clearanceRadius * 2f);
            _minimumLootSpacing = Mathf.Max(minimumLootSpacing, 0f);
            _floorOffset = Mathf.Max(floorOffset, 0f);
        }

        public Transform PlayerSpawnPoint => _playerSpawnPoint;
        public LayerMask FloorLayer => _floorLayer;
        public LayerMask ObstacleLayer => _obstacleLayer;
        public float MinimumPlayerSpawnDistance => _minimumPlayerSpawnDistance;
        public int PositionSearchAttempts => _positionSearchAttempts;
        public float FloorCheckHeight => _floorCheckHeight;
        public float FloorCheckDistance => _floorCheckDistance;
        public float MaximumFloorAngle => _maximumFloorAngle;
        public float ClearanceRadius => _clearanceRadius;
        public float ClearanceHeight => _clearanceHeight;
        public float MinimumLootSpacing => _minimumLootSpacing;
        public float FloorOffset => _floorOffset;
    }
}
