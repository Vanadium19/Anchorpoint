using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "PlacementConfig", menuName = "Game/Configs/Constructing/PlacementConfig")]
    public class PlacementConfig : ScriptableObject
    {
        [Header("Input")]
        [SerializeField] private float inputTolerance = 0.01f;

        [Header("Distance")]
        [SerializeField] private float minPlacementDistance = 3f;
        [SerializeField] private float maxPlacementDistance = 20f;

        [Header("Sensitivity")]
        [SerializeField] private float scrollSensitivity = 5f;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Preview")]
        [SerializeField] private float previewYOffset = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float previewAlpha = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float smoothSpeed = 0.5f;
        [SerializeField] private float deadZoneThreshold = 0.05f;
        [SerializeField] private float ceilingOffset = 0.1f;
        [Range(0f, 2f)]
        [SerializeField] private float ceilingAngleFactor = 0.5f;
        [Range(0f, 2f)]
        [SerializeField] private float wallAngleFactor = 0.5f;
        [SerializeField] private float wallOffset = 0.1f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionLayer = ~0;
        [SerializeField] private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

        public float InputTolerance => inputTolerance;
        public float MinPlacementDistance => minPlacementDistance;
        public float MaxPlacementDistance => maxPlacementDistance;
        public float ScrollSensitivity => scrollSensitivity;
        public float RotationSpeed => rotationSpeed;
        public float PreviewYOffset => previewYOffset;
        public float PreviewAlpha => previewAlpha;
        public float SmoothSpeed => smoothSpeed;
        public float DeadZoneThreshold => deadZoneThreshold;
        public float CeilingOffset => ceilingOffset;
        public float CeilingAngleFactor => ceilingAngleFactor;
        public float WallAngleFactor => wallAngleFactor;
        public float WallOffset => wallOffset;
        public LayerMask CollisionLayer => collisionLayer;
        public QueryTriggerInteraction QueryTriggerInteraction => queryTriggerInteraction;
    }
}
