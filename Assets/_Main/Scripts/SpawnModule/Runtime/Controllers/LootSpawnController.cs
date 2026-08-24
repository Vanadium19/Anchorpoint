using System.Collections.Generic;
using System.Linq;
using InventoryModule;
using UnityEngine;

namespace SpawnModule
{
    public class LootSpawnController
    {
        private readonly LevelLootSpawnPointsView _spawnPointsView;
        private readonly ILootFactory _factory;
        private readonly LootSpawnValidationConfig _validationConfig;
        private readonly List<Vector3> _spawnedPositions = new();

        private Vector3 _playerSpawnPosition;

        public LootSpawnController(
            LevelLootSpawnPointsView spawnPointsView,
            ILootFactory factory,
            LootSpawnValidationConfig validationConfig)
        {
            _spawnPointsView = spawnPointsView;
            _factory = factory;
            _validationConfig = validationConfig;
        }

        public void SpawnInitialLoot()
        {
            _playerSpawnPosition = _validationConfig.PlayerSpawnPoint.position;
            SpawnLoot();
        }

        private void SpawnLoot()
        {
            if (_spawnPointsView == null || _spawnPointsView.SpawnPoints == null)
                return;

            _spawnedPositions.Clear();

            var spawnPoints = _spawnPointsView.SpawnPoints;

            foreach (var spawnPoint in spawnPoints)
                SpawnAtPoint(spawnPoint);
        }

        private void SpawnAtPoint(LootSpawnPointView spawnPoint)
        {
            if (spawnPoint == null)
                return;

            var entries = spawnPoint.GetLootEntries();

            if (entries == null || entries.Count == 0)
                return;

            var totalWeight = entries.Sum(entry => Mathf.Max(entry.Weight, 0));

            if (totalWeight == 0)
                return;

            var itemsToSpawn = GetItemsToSpawn(spawnPoint);

            for (var itemIndex = 0; itemIndex < itemsToSpawn; itemIndex++)
            {
                var selectedEntry = SelectByWeight(entries, totalWeight);

                if (selectedEntry?.Item == null)
                    continue;

                SpawnEntry(spawnPoint, selectedEntry);
            }
        }

        private void SpawnEntry(LootSpawnPointView spawnPoint, LootEntry entry)
        {
            var count = Random.Range(entry.MinCount, entry.MaxCount + 1);

            if (entry.IsStacked)
            {
                if (TryGetSpawnPosition(spawnPoint, out var stackedPosition))
                    _factory.Create(stackedPosition, entry.Item, count, true);

                return;
            }

            for (var itemIndex = 0; itemIndex < count; itemIndex++)
            {
                if (!TryGetSpawnPosition(spawnPoint, out var position))
                    return;

                _factory.Create(position, entry.Item, 1);
            }
        }

        private int GetItemsToSpawn(LootSpawnPointView spawnPoint)
        {
            var minimumCount = spawnPoint.GetMinItemsToSpawn();
            var maximumCount = spawnPoint.GetMaxItemsToSpawn();

            return minimumCount == maximumCount
                ? minimumCount
                : Random.Range(minimumCount, maximumCount + 1);
        }

        private LootEntry SelectByWeight(List<LootEntry> entries, int totalWeight)
        {
            var randomWeight = Random.Range(1, totalWeight + 1);

            for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                var entryWeight = Mathf.Max(entries[entryIndex].Weight, 0);

                if (randomWeight <= entryWeight)
                    return entries[entryIndex];

                randomWeight -= entryWeight;
            }

            return entries[entries.Count - 1];
        }

        private bool TryGetSpawnPosition(LootSpawnPointView spawnPoint, out Vector3 position)
        {
            for (var attemptIndex = 0; attemptIndex < _validationConfig.PositionSearchAttempts; attemptIndex++)
            {
                var randomPosition = GetRandomPosition(spawnPoint);

                if (!TryGetFloorPosition(randomPosition, out var floorPosition))
                    continue;

                var candidatePosition = floorPosition + Vector3.up * _validationConfig.FloorOffset;

                if (!IsFarEnoughFromPlayerSpawn(candidatePosition))
                    continue;

                if (!IsFarEnoughFromSpawnedLoot(candidatePosition))
                    continue;

                if (!IsSpaceAvailable(candidatePosition))
                    continue;

                _spawnedPositions.Add(candidatePosition);
                position = candidatePosition;
                return true;
            }

            position = default;
            return false;
        }

        private Vector3 GetRandomPosition(LootSpawnPointView spawnPoint)
        {
            var radius = spawnPoint.GetSpawnRadius();

            if (radius <= 0f)
                return spawnPoint.transform.position;

            var randomCircle = Random.insideUnitCircle * radius;
            var spawnPointPosition = spawnPoint.transform.position;

            return new Vector3(
                spawnPointPosition.x + randomCircle.x,
                spawnPointPosition.y,
                spawnPointPosition.z + randomCircle.y);
        }

        private bool TryGetFloorPosition(Vector3 position, out Vector3 floorPosition)
        {
            var rayOrigin = position + Vector3.up * _validationConfig.FloorCheckHeight;
            var rayDistance = _validationConfig.FloorCheckHeight + _validationConfig.FloorCheckDistance;

            if (!Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out var hit,
                    rayDistance,
                    _validationConfig.FloorLayer,
                    QueryTriggerInteraction.Ignore))
            {
                floorPosition = default;
                return false;
            }

            var floorAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (floorAngle > _validationConfig.MaximumFloorAngle)
            {
                floorPosition = default;
                return false;
            }

            floorPosition = hit.point;
            return true;
        }

        private bool IsFarEnoughFromPlayerSpawn(Vector3 position)
        {
            var positionOnPlane = new Vector2(position.x, position.z);
            var playerSpawnOnPlane = new Vector2(_playerSpawnPosition.x, _playerSpawnPosition.z);
            var minimumDistance = _validationConfig.MinimumPlayerSpawnDistance;

            return (positionOnPlane - playerSpawnOnPlane).sqrMagnitude >= minimumDistance * minimumDistance;
        }

        private bool IsFarEnoughFromSpawnedLoot(Vector3 position)
        {
            var minimumSpacing = _validationConfig.MinimumLootSpacing;
            var minimumSpacingSquared = minimumSpacing * minimumSpacing;

            for (var positionIndex = 0; positionIndex < _spawnedPositions.Count; positionIndex++)
            {
                if ((position - _spawnedPositions[positionIndex]).sqrMagnitude < minimumSpacingSquared)
                    return false;
            }

            return true;
        }

        private bool IsSpaceAvailable(Vector3 position)
        {
            var radius = _validationConfig.ClearanceRadius;
            var bottom = position + Vector3.up * radius;
            var top = position + Vector3.up * (_validationConfig.ClearanceHeight - radius);

            return !Physics.CheckCapsule(
                bottom,
                top,
                radius,
                _validationConfig.ObstacleLayer,
                QueryTriggerInteraction.Ignore);
        }
    }
}
