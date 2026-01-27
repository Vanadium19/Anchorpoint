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

        private float _speed;
        private float _verticalVelocity;

        public MoveComponent(CharacterController characterController, float speed, float jumpHeight, float gravity)
        {
            _characterController = characterController;
            _transform = characterController.transform;

            _jumpHeight = jumpHeight;
            _gravity = gravity;

            _speed = speed;
        }

        public void Move(Vector2 direction, bool isJumping)
        {
            var movement = direction.x * _transform.right + direction.y * _transform.forward;
            movement *= _speed;

            _verticalVelocity = CalculateVerticalVelocity(isJumping);
            movement.y = _verticalVelocity;

            _characterController.Move(movement * Time.deltaTime);
        }

        public void SetSpeed(float value)
        {
            _speed = value;
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