using UnityEngine;
using WeaponModule.Configs;

namespace WeaponModule.View.Procedural
{
    public class RecoilProcessor
    {
        // Текущие значения
        private Vector3 _targetRotation;
        private Vector3 _currentRotation;
        private Vector3 _targetPosition; // Только для оружия
        private Vector3 _currentPosition;

        private float _snappiness;
        private float _returnSpeed;

        public Vector3 CurrentRotation => _currentRotation;
        public Vector3 CurrentPosition => _currentPosition;

        // Вызываем это каждый кадр
        public void Update(float deltaTime)
        {
            _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _returnSpeed * deltaTime);
            _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * deltaTime);

            _targetPosition = Vector3.Lerp(_targetPosition, Vector3.zero, _returnSpeed * deltaTime);
            _currentPosition = Vector3.Lerp(_currentPosition, _targetPosition, _snappiness * deltaTime);
        }

        public void Fire(WeaponConfig.RecoilSettings settings)
        {
            _snappiness = settings.Snappiness;
            _returnSpeed = settings.ReturnSpeed;

            _targetRotation += new Vector3(
                settings.RecoilRotation.x,
                Random.Range(-settings.RecoilRotation.y, settings.RecoilRotation.y),
                Random.Range(-settings.RecoilRotation.z, settings.RecoilRotation.z));

            _targetPosition += new Vector3(0, 0, -settings.KickBackZ);
        }

        // Перегрузка для камеры (там нет позиции kickback)
        public void FireCamera(WeaponConfig.CameraRecoilSettings settings)
        {
            _snappiness = settings.Snappiness;
            _returnSpeed = settings.ReturnSpeed;

            _targetRotation += new Vector3(
                settings.RecoilAmount.x,
                Random.Range(-settings.RecoilAmount.y, settings.RecoilAmount.y),
                Random.Range(-settings.RecoilAmount.z, settings.RecoilAmount.z));
        }
    }
}