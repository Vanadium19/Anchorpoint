using UnityEngine;

namespace WeaponModule.Content // Или WeaponModule.View
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private GameObject hitEffect; // Один общий эффект для всего

        private float _damage;
        private Rigidbody _rb;
        private TrailRenderer _trail;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _trail = GetComponent<TrailRenderer>();

            if (_trail != null)
                _trail.enabled = false; // Выключаем трейл, чтобы он не рисовался из (0,0,0)
        }

        public void Setup(float damage, float bulletSpeed, float inheritFactor, Vector3 shooterVelocity)
        {
            _damage = damage;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;

                // Вектор пули (куда смотрит ствол * скорость пули)
                Vector3 bulletVel = transform.forward * bulletSpeed;

                // Вектор игрока (куда бежим * коэффициент)
                Vector3 playerVel = shooterVelocity * inheritFactor;

                // ИТОГОВАЯ СКОРОСТЬ (Сложение векторов)
                _rb.linearVelocity = bulletVel + playerVel;
            }

            if (_trail != null)
            {
                _trail.Clear();
                _trail.enabled = true; // Включаем СРАЗУ, без задержки
            }
            Destroy(gameObject, lifeTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // --- БЛОК НАНЕСЕНИЯ УРОНА ---
            // Тут мы ищем компоненты здоровья. 
            // Пока используем SendMessage или интерфейс, если он у тебя есть.
            // В будущем тут будет вызов HealthSystem.

            // Пример (раскомментируй, когда перенесешь Health):
            /*
            if (collision.gameObject.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(_damage);
            }
            */

            // Для теста можно вывести лог
            // Debug.Log($"Hit: {collision.gameObject.name} for {_damage} dmg");

            // --- ВИЗУАЛ ПОПАДАНИЯ ---
            if (hitEffect != null)
            {
                ContactPoint contact = collision.contacts[0];

                // Спавним эффект чуть выше точки попадания и поворачиваем по нормали
                GameObject effect = Instantiate(hitEffect, contact.point + contact.normal * 0.05f, Quaternion.LookRotation(contact.normal));

                Destroy(effect, 2f);
            }

            Destroy(gameObject);
        }
    }
}