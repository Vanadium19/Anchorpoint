using UnityEngine;

namespace PlayerModule.View
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform playerCamera;

        public bool IsGrounded => characterController.isGrounded;
        public float CurrentHeight => characterController.height;
        public Transform CameraRoot => cameraRoot;

        // Метод для перемещения (вызывается контроллером)
        public void Move(Vector3 motion)
        {
            characterController.Move(motion);
        }

        // Вращение камеры (Controller считает углы, View применяет)
        public void SetRotation(float xRotation, float yRotation)
        {
            // Вращаем камеру вверх-вниз
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0, 0);
            // Вращаем тело персонажа влево-вправо
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        // Установка высоты коллайдера
        public void SetControllerHeight(float height, float centerY)
        {
            characterController.height = height;
            characterController.center = Vector3.up * centerY;
        }

        // Локальное положение камеры (для приседания и наклонов)
        public void SetCameraLocalPosition(Vector3 position)
        {
            cameraRoot.localPosition = position;
        }

        // Локальный поворот рута камеры (для наклонов Q/E)
        public void SetCameraRootRotation(Quaternion rotation)
        {
            cameraRoot.localRotation = rotation;
        }
    }
}