using System;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class LootSpawnInstaller : MonoInstaller
    {
        [Header("References")]
        [SerializeField] private LevelLootSpawnPointsView spawnPointsView;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Player Spawn Distance")]
        [SerializeField, Min(0f)] private float minimumPlayerSpawnDistance = 10f;

        [Header("Floor Check")]
        [SerializeField] private LayerMask floorLayer = ~0;
        [SerializeField, Min(0f)] private float floorCheckHeight = 2f;
        [SerializeField, Min(0.01f)] private float floorCheckDistance = 5f;
        [SerializeField, Range(0f, 90f)] private float maximumFloorAngle = 45f;
        [SerializeField, Min(0f)] private float floorOffset = 0.05f;

        [Header("Availability")]
        [SerializeField] private LayerMask obstacleLayer = ~0;
        [SerializeField, Min(0.01f)] private float clearanceRadius = 0.35f;
        [SerializeField, Min(0.02f)] private float clearanceHeight = 0.7f;
        [SerializeField, Min(0f)] private float minimumLootSpacing = 0.75f;
        [SerializeField, Min(1)] private int positionSearchAttempts = 20;

        public override void InstallBindings()
        {
            if (playerSpawnPoint == null)
                throw new InvalidOperationException("Player spawn point is not assigned in LootSpawnInstaller.");

            var validationConfig = new LootSpawnValidationConfig(
                playerSpawnPoint,
                floorLayer,
                obstacleLayer,
                minimumPlayerSpawnDistance,
                positionSearchAttempts,
                floorCheckHeight,
                floorCheckDistance,
                maximumFloorAngle,
                clearanceRadius,
                clearanceHeight,
                minimumLootSpacing,
                floorOffset);

            Container.BindInstance(spawnPointsView).AsSingle();
            Container.BindInstance(validationConfig).AsSingle();

            Container.Bind<ILootFactory>()
                .To<LootFactory>()
                .AsSingle();

            Container.BindInterfacesTo<LootSpawnController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
