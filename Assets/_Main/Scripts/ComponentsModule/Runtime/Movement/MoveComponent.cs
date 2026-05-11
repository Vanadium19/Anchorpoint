using UnityEngine;

namespace ComponentsModule
{
    public class MoveComponent : IMoveComponent
    {
        private const float GroundingForce = -2f;

        private readonly CharacterController _characterController;
        private readonly Transform _transform;

        private readonly float _jumpHeight;
        private readonly float _gravity;

        private readonly float _baseSpeed;
        private float _speedMultiplier = 1f;
        private float _verticalVelocity;

        public float BaseSpeed => _baseSpeed;
        public float CurrentSpeed => _baseSpeed * _speedMultiplier;

        public MoveComponent(CharacterController characterController, float speed, float jumpHeight, float gravity)
        {
            _characterController = characterController;
            _transform = characterController.transform;

            _jumpHeight = jumpHeight;
            _gravity = gravity;

            _baseSpeed = speed;
        }

        public void Move(Vector2 direction, bool isJumping, float baseSpeed)
        {
            var speed = baseSpeed * _speedMultiplier;
            var movement = direction.x * _transform.right + direction.y * _transform.forward;
            movement *= speed;

            _verticalVelocity = CalculateVerticalVelocity(isJumping);
            movement.y = _verticalVelocity;

            _characterController.Move(movement * Time.deltaTime);
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier;
        }

        public void ResetSpeedMultiplier()
        {
            _speedMultiplier = 1f;
        }

        private float CalculateVerticalVelocity(bool isJumping)
        {
            var verticalVelocity = _verticalVelocity;
            var isGrounded = _characterController.isGrounded;

            if (isGrounded)
            {
                if (isJumping)
                {
                    verticalVelocity = Mathf.Sqrt(_jumpHeight * GroundingForce * _gravity);
                }
                else if (verticalVelocity < 0)
                {
                    verticalVelocity = GroundingForce;
                }
            }
            else
            {
                verticalVelocity += _gravity * Time.deltaTime;
            }

            return verticalVelocity;
        }
    }
}