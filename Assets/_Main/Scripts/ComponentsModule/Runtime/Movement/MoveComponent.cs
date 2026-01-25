using UnityEngine;

namespace ComponentsModule
{
    public class MoveComponent : IMoveComponent
    {
        private readonly CharacterController _characterController;
        private readonly Transform _transform;

        private readonly float _jumpHeight;
        private readonly float _gravity;

        private float _speed;
        private float _translationY;

        public MoveComponent(CharacterController characterController, float speed, float jumpHeight, float gravity)
        {
            _characterController = characterController;
            _transform = characterController.transform;

            _jumpHeight = jumpHeight;
            _gravity = gravity;

            _speed = speed;
        }

        public void Move(Vector2 direction, bool jumped)
        {
            var translation = direction.x * _transform.right + direction.y * _transform.forward;
            translation *= _speed;

            _translationY = GetYTranslation(jumped);
            translation.y = _translationY;

            _characterController.Move(translation * Time.deltaTime);
        }

        //TODO: Обновлять при приседание
        public void SetSpeed(float value)
        {
            _speed = value;
        }

        //FIXME: Magic numbers
        //TODO: Логика приседаний
        private float GetYTranslation(bool jumped)
        {
            var y = _translationY;
            var isGrounded = _characterController.isGrounded;

            if (jumped && isGrounded /* && !isCrouching*/)
                y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

            if (!isGrounded)
                y += _gravity * Time.deltaTime;
            else if (y < 0)
                y = -2f;

            return y;
        }
    }
}