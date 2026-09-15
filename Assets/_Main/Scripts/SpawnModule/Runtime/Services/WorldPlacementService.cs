using UnityEngine;
using UnityEngine.AI;

namespace SpawnModule
{
    /// <summary>Finds a free, floor-supported, player-reachable world position around a given origin.</summary>
    /// <remarks>
    /// Runs the same floor, obstacle-clearance and player-distance checks as <see cref="LootSpawnController"/>,
    /// plus a NavMesh sample that rejects points the player could not path to and snaps the accepted point onto
    /// the mesh. Reusable by any caller that needs to place something in the world rather than at an authored
    /// spawn point, such as <see cref="SpawnEventSiteAction"/>.
    /// </remarks>
    public class WorldPlacementService
    {
        private const float NavMeshSampleRadius = 1f;
        private const float ProjectionSampleRadius = 3f;

        private readonly LootSpawnValidationConfig _validationConfig;

        /// <summary>Creates the service over the scene's shared placement validation settings.</summary>
        public WorldPlacementService(LootSpawnValidationConfig validationConfig)
        {
            _validationConfig = validationConfig;
        }

        /// <summary>Searches for a valid position around <paramref name="origin"/>, returning the first one found.</summary>
        /// <remarks>
        /// Tries up to <see cref="LootSpawnValidationConfig.PositionSearchAttempts"/> random points inside
        /// <paramref name="searchRadius"/> of <paramref name="origin"/>. A candidate must land on the floor layer
        /// within the configured angle and check distance, sit at least <paramref name="minimumPlayerDistance"/>
        /// from the player spawn point on the XZ plane, have no obstacle inside a capsule of
        /// <paramref name="clearanceRadius"/>/<paramref name="clearanceHeight"/>, and sample onto the NavMesh;
        /// the returned position is snapped to that NavMesh sample.
        /// </remarks>
        public bool TryFindPlacement(
            Vector3 origin,
            float searchRadius,
            float clearanceRadius,
            float clearanceHeight,
            float minimumPlayerDistance,
            out Vector3 position)
        {
            for (var attemptIndex = 0; attemptIndex < _validationConfig.PositionSearchAttempts; attemptIndex++)
            {
                var randomPosition = GetRandomPosition(origin, searchRadius);

                if (!TryGetFloorPosition(randomPosition, out var floorPosition))
                    continue;

                var candidatePosition = floorPosition + Vector3.up * _validationConfig.FloorOffset;

                if (!IsFarEnoughFromPlayerSpawn(candidatePosition, minimumPlayerDistance))
                    continue;

                if (!IsSpaceAvailable(candidatePosition, clearanceRadius, clearanceHeight))
                    continue;

                if (!TryGetNavMeshPosition(candidatePosition, NavMeshSampleRadius, out var navMeshPosition))
                    continue;

                position = navMeshPosition;
                return true;
            }

            position = default;
            return false;
        }

        /// <summary>Moves a position onto the nearest NavMesh point, reporting whether one is close enough.</summary>
        /// <remarks>
        /// An authored point can sit just off the mesh — against a wall, on a prop — where an agent cannot be
        /// created. Callers that spawn navigating characters project their positions first and skip the ones
        /// with no mesh nearby.
        /// </remarks>
        public bool TryProjectToNavMesh(Vector3 position, out Vector3 projectedPosition) =>
            TryGetNavMeshPosition(position, ProjectionSampleRadius, out projectedPosition);

        private static Vector3 GetRandomPosition(Vector3 origin, float searchRadius)
        {
            if (searchRadius <= 0f)
                return origin;

            var randomCircle = Random.insideUnitCircle * searchRadius;

            return new Vector3(origin.x + randomCircle.x, origin.y, origin.z + randomCircle.y);
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

        private bool IsFarEnoughFromPlayerSpawn(Vector3 position, float minimumPlayerDistance)
        {
            var playerSpawnPosition = _validationConfig.PlayerSpawnPoint.position;
            var positionOnPlane = new Vector2(position.x, position.z);
            var playerSpawnOnPlane = new Vector2(playerSpawnPosition.x, playerSpawnPosition.z);

            return (positionOnPlane - playerSpawnOnPlane).sqrMagnitude >= minimumPlayerDistance * minimumPlayerDistance;
        }

        private bool IsSpaceAvailable(Vector3 position, float clearanceRadius, float clearanceHeight)
        {
            var bottom = position + Vector3.up * clearanceRadius;
            var top = position + Vector3.up * (clearanceHeight - clearanceRadius);

            return !Physics.CheckCapsule(
                bottom,
                top,
                clearanceRadius,
                _validationConfig.ObstacleLayer,
                QueryTriggerInteraction.Ignore);
        }

        private static bool TryGetNavMeshPosition(Vector3 position, float sampleRadius, out Vector3 navMeshPosition)
        {
            if (NavMesh.SamplePosition(position, out var navMeshHit, sampleRadius, NavMesh.AllAreas))
            {
                navMeshPosition = navMeshHit.position;
                return true;
            }

            navMeshPosition = default;
            return false;
        }
    }
}
