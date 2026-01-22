using UnityEngine;
using Zenject;
using PlayerModule.Configs;
using PlayerModule.View;
using InputModule.Core;

namespace PlayerModule.Controllers
{
    public class PlayerController : IInitializable, ITickable
    {
        private readonly PlayerConfig _config;
        private readonly PlayerView _view;
        private readonly IGameInput _input;

        // Состояние
        private Vector3 _moveDirection;
        private float _rotationX;
        private float _rotationY;
        private float _currentLean;

        public PlayerController(PlayerConfig config, PlayerView view, IGameInput input)
        {
            _config = config;
            _view = view;
            _input = input;
        }

        public void Initialize()
        {
            // Инициализация при старте
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Начальный поворот берем от View
            _rotationY = _view.transform.eulerAngles.y;
        }

        public void Tick()
        {
            HandleMovement();
            HandleRotation();
            HandleCrouch();
            HandleLean();
        }

        private void HandleMovement()
        {
            bool isCrouching = _input.IsCrouchPressed;
            float targetSpeed = isCrouching ? _config.CrouchSpeed : _config.WalkSpeed;

            Vector2 inputDir = _input.MoveInput;

            // Расчет направления относительно поворота персонажа
            // Важно: View.transform дает нам направление, так как View - это объект на сцене
            Transform tr = _view.transform;
            Vector3 forward = tr.forward;
            Vector3 right = tr.right;

            float curSpeedX = targetSpeed * inputDir.y;
            float curSpeedY = targetSpeed * inputDir.x;

            float movementDirectionY = _moveDirection.y;

            // Итоговый вектор движения (плоский)
            Vector3 flatMovement = (forward * curSpeedX) + (right * curSpeedY);
            _moveDirection = flatMovement;
            _moveDirection.y = movementDirectionY;

            // Прыжок
            if (_input.IsJumpPressed && _view.IsGrounded && !isCrouching)
            {
                _moveDirection.y = Mathf.Sqrt(_config.JumpHeight * -2f * _config.Gravity);
            }

            // Гравитация
            if (!_view.IsGrounded)
            {
                _moveDirection.y += _config.Gravity * Time.deltaTime;
            }
            else if (_moveDirection.y < 0)
            {
                // Небольшая сила прижима к земле, чтобы IsGrounded работал стабильно
                _moveDirection.y = -2f;
            }

            // Отправляем команду View
            _view.Move(_moveDirection * Time.deltaTime);
        }

        private void HandleRotation()
        {
            Vector2 mouseDelta = _input.LookInput;

            _rotationX += -mouseDelta.y * _config.MouseSensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -_config.LookXLimit, _config.LookXLimit);

            _rotationY += mouseDelta.x * _config.MouseSensitivity;

            _view.SetRotation(_rotationX, _rotationY);
        }

        private void HandleCrouch()
        {
            bool isCrouching = _input.IsCrouchPressed;

            float targetHeight = isCrouching ? _config.CrouchHeight : _config.StandHeight;
            float targetEyeHeight = isCrouching ? _config.EyeHeightCrouching : _config.EyeHeightStanding;

            // Плавно меняем высоту контроллера
            float newHeight = Mathf.Lerp(_view.CurrentHeight, targetHeight, Time.deltaTime * _config.CrouchTransitionSpeed);
            _view.SetControllerHeight(newHeight, newHeight / 2f);

            // Плавно меняем позицию камеры
            Vector3 currentCamPos = _view.CameraRoot.localPosition;
            Vector3 targetCamPos = new Vector3(0, targetEyeHeight, 0);

            Vector3 newCamPos = Vector3.Lerp(currentCamPos, targetCamPos, Time.deltaTime * _config.CrouchTransitionSpeed);
            _view.SetCameraLocalPosition(newCamPos);
        }

        private void HandleLean()
        {
            float targetLean = _input.LeanInput;

            _currentLean = Mathf.Lerp(_currentLean, targetLean, Time.deltaTime * _config.LeanSpeed);

            // Поворот (Roll)
            Quaternion targetRotation = Quaternion.Euler(0, 0, -_currentLean * _config.LeanAngle);
            _view.SetCameraRootRotation(targetRotation);

            // Смещение камеры вбок
            Vector3 camPos = _view.CameraRoot.localPosition;
            camPos.x = _currentLean * _config.LeanOffset;
            _view.SetCameraLocalPosition(camPos);
        }
    }
}