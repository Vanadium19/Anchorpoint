using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RandomEventsModule;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace SpawnModule
{
    /// <summary>Spawns an event site prefab with its guarding enemies and loot at a valid, reachable map position.</summary>
    /// <remarks>
    /// The origin for the placement search is a random scene spawn point from <see cref="LevelSpawnPointsView"/>;
    /// several origins are tried in turn until <see cref="WorldPlacementService"/> accepts one or all are
    /// exhausted. Once placed, guards and loot use the site's own <see cref="EventSiteView"/> points when it has
    /// any, falling back to scattering around the site otherwise; a site prefab without the view scatters both.
    /// Guards are the scene's default enemy unless the action names its own guard prefab.
    /// The action completes immediately after spawning and returns <c>true</c>. It returns <c>false</c> without
    /// spawning anything when there is no site prefab, no scene spawn point, or no valid placement, so a map with
    /// nowhere to put the site does not burn the event's cooldown.
    /// </remarks>
    public class SpawnEventSiteAction : RandomEventActionBase
    {
        private const int MaxOriginAttempts = 5;
        private const float DefaultClearanceRadius = 2f;
        private const float DefaultClearanceHeight = 3f;

        private readonly IEnemyFactory _enemyFactory;
        private readonly ILootFactory _lootFactory;
        private readonly LevelSpawnPointsView _spawnPoints;
        private readonly WorldPlacementService _placement;
        private readonly DiContainer _container;
        private readonly GameObject _sitePrefab;
        private readonly GameObject _guardPrefab;
        private readonly int _guardCount;
        private readonly float _guardScatterRadius;
        private readonly GameObject _occupantPrefab;
        private readonly int _occupantCount;
        private readonly float _occupantScatterRadius;
        private readonly List<LootEntry> _lootEntries;
        private readonly int _minimumLootDrops;
        private readonly int _maximumLootDrops;
        private readonly float _searchRadius;
        private readonly float _minimumPlayerDistance;

        /// <summary>Creates the action with its site prefab, guard and loot settings, and placement search range.</summary>
        public SpawnEventSiteAction(
            IEnemyFactory enemyFactory,
            ILootFactory lootFactory,
            LevelSpawnPointsView spawnPoints,
            WorldPlacementService placement,
            DiContainer container,
            GameObject sitePrefab,
            GameObject guardPrefab,
            int guardCount,
            float guardScatterRadius,
            GameObject occupantPrefab,
            int occupantCount,
            float occupantScatterRadius,
            List<LootEntry> lootEntries,
            int minimumLootDrops,
            int maximumLootDrops,
            float searchRadius,
            float minimumPlayerDistance)
        {
            _enemyFactory = enemyFactory;
            _lootFactory = lootFactory;
            _spawnPoints = spawnPoints;
            _placement = placement;
            _container = container;
            _sitePrefab = sitePrefab;
            _guardPrefab = guardPrefab;
            _guardCount = guardCount;
            _guardScatterRadius = guardScatterRadius;
            _occupantPrefab = occupantPrefab;
            _occupantCount = occupantCount;
            _occupantScatterRadius = occupantScatterRadius;
            _lootEntries = lootEntries;
            _minimumLootDrops = minimumLootDrops;
            _maximumLootDrops = maximumLootDrops;
            _searchRadius = searchRadius;
            _minimumPlayerDistance = minimumPlayerDistance;
        }

        /// <inheritdoc/>
        public override UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            if (_sitePrefab == null)
                return UniTask.FromResult(false);

            var points = _spawnPoints.SpawnPoints;

            if (points == null || points.Count == 0)
                return UniTask.FromResult(false);

            if (!TryFindSitePlacement(points, out var position))
                return UniTask.FromResult(false);

            SpawnSite(position);

            return UniTask.FromResult(true);
        }

        private bool TryFindSitePlacement(List<SpawnPointView> points, out Vector3 position)
        {
            GetSiteClearance(out var clearanceRadius, out var clearanceHeight);

            var originAttempts = Mathf.Min(points.Count, MaxOriginAttempts);
            var firstOriginIndex = Random.Range(0, points.Count);

            for (var attemptIndex = 0; attemptIndex < originAttempts; attemptIndex++)
            {
                var origin = points[(firstOriginIndex + attemptIndex) % points.Count].Position;

                if (_placement.TryFindPlacement(origin, _searchRadius, clearanceRadius, clearanceHeight, _minimumPlayerDistance, out position))
                    return true;
            }

            position = default;
            return false;
        }

        private void GetSiteClearance(out float clearanceRadius, out float clearanceHeight)
        {
            var siteView = _sitePrefab.GetComponent<EventSiteView>();

            clearanceRadius = siteView != null ? siteView.ClearanceRadius : DefaultClearanceRadius;
            clearanceHeight = siteView != null ? siteView.ClearanceHeight : DefaultClearanceHeight;
        }

        private void SpawnSite(Vector3 position)
        {
            var rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            var siteObject = _container.InstantiatePrefab(_sitePrefab, position, rotation, null);
            var siteView = siteObject.GetComponent<EventSiteView>();

            SpawnGuards(position, siteView);
            SpawnOccupants(position, siteView);
            SpawnLoot(position, siteView);
        }

        private void SpawnGuards(Vector3 sitePosition, EventSiteView siteView)
        {
            var guardPoints = siteView != null ? siteView.GuardPoints : null;
            var hasGuardPoints = guardPoints != null && guardPoints.Count > 0;

            for (var guardIndex = 0; guardIndex < _guardCount; guardIndex++)
            {
                var guardPoint = hasGuardPoints ? guardPoints[guardIndex % guardPoints.Count] : null;
                var position = guardPoint != null ? guardPoint.position : sitePosition + GetScatterOffset(_guardScatterRadius);

                if (!_placement.TryProjectToNavMesh(position, out var navMeshPosition))
                    continue;

                if (_guardPrefab != null)
                    _enemyFactory.Create(_guardPrefab, navMeshPosition);
                else
                    _enemyFactory.Create(navMeshPosition);
            }
        }

        private void SpawnOccupants(Vector3 sitePosition, EventSiteView siteView)
        {
            if (_occupantPrefab == null)
                return;

            var occupantPoints = siteView != null ? siteView.OccupantPoints : null;
            var hasOccupantPoints = occupantPoints != null && occupantPoints.Count > 0;

            for (var occupantIndex = 0; occupantIndex < _occupantCount; occupantIndex++)
            {
                var occupantPoint = hasOccupantPoints ? occupantPoints[occupantIndex % occupantPoints.Count] : null;
                var position = occupantPoint != null ? occupantPoint.position : sitePosition + GetScatterOffset(_occupantScatterRadius);
                var rotation = occupantPoint != null ? occupantPoint.rotation : Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                _container.InstantiatePrefab(_occupantPrefab, position, rotation, null);
            }
        }

        private void SpawnLoot(Vector3 sitePosition, EventSiteView siteView)
        {
            if (_lootEntries == null || _lootEntries.Count == 0)
                return;

            var totalWeight = _lootEntries.Sum(entry => Mathf.Max(entry.Weight, 0));

            if (totalWeight == 0)
                return;

            var lootPoints = siteView != null ? siteView.LootPoints : null;
            var hasLootPoints = lootPoints != null && lootPoints.Count > 0;
            var dropCount = Random.Range(_minimumLootDrops, _maximumLootDrops + 1);

            for (var dropIndex = 0; dropIndex < dropCount; dropIndex++)
            {
                var entry = SelectByWeight(totalWeight);

                if (entry?.Item == null)
                    continue;

                var lootPoint = hasLootPoints ? lootPoints[dropIndex % lootPoints.Count] : null;
                var position = lootPoint != null ? lootPoint.position : sitePosition + GetScatterOffset(_guardScatterRadius);
                var count = Random.Range(entry.MinCount, entry.MaxCount + 1);

                _lootFactory.Create(position, entry.Item, count, entry.IsStacked);
            }
        }

        private LootEntry SelectByWeight(int totalWeight)
        {
            var randomWeight = Random.Range(1, totalWeight + 1);

            for (var entryIndex = 0; entryIndex < _lootEntries.Count; entryIndex++)
            {
                var entryWeight = Mathf.Max(_lootEntries[entryIndex].Weight, 0);

                if (randomWeight <= entryWeight)
                    return _lootEntries[entryIndex];

                randomWeight -= entryWeight;
            }

            return _lootEntries[_lootEntries.Count - 1];
        }

        private static Vector3 GetScatterOffset(float radius)
        {
            var offset = Random.insideUnitCircle * radius;

            return new Vector3(offset.x, 0f, offset.y);
        }
    }
}
