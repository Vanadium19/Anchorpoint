using UnityEngine;

namespace BuildingModule
{
    public sealed class DefenseProjectileView : MonoBehaviour
    {
        private Vector3 _targetPosition;
        private float _speed;
        private bool _isInitialized;

        public void Initialize(Vector3 targetPosition, float speed)
        {
            _targetPosition = targetPosition;
            _speed = Mathf.Max(speed, 0.1f);
            _isInitialized = true;

            var direction = _targetPosition - transform.position;

            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPosition,
                _speed * Time.deltaTime);

            if ((transform.position - _targetPosition).sqrMagnitude > 0.0025f)
                return;

            Destroy(gameObject);
        }
    }
}
