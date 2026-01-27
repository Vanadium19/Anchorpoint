using ComponentsModule;
using UnityEngine;

namespace PlayerModule
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float jumpHeight = 1.2f;
        
        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float lookXLimit = 85f;
        
        [Header("Lean")]
        [SerializeField] private float leanAngle = 15f;
        [SerializeField] private float leanOffset = 0.5f;
        [SerializeField] private float leanSpeed = 10f;
        
        [Header("Crouch")]
        [SerializeField] private CrouchData crouchData;
        
        public float MaxHealth => maxHealth;
        public float WalkSpeed => walkSpeed;
        public float CrouchSpeed => crouchSpeed;
        
        public float Gravity => gravity;
        public float JumpHeight => jumpHeight;
        
        public float MouseSensitivity => mouseSensitivity;
        public float LookXLimit => lookXLimit;

        public float LeanAngle => leanAngle;
        public float LeanOffset => leanOffset;
        public float LeanSpeed => leanSpeed;
        
        public CrouchData CrouchData => crouchData;
    }
}