using UnityEngine;
using Zenject;

namespace ComponentsModule
{
    public class DamageHandler : MonoBehaviour, IDamageable
    {
        private IDamageable _health;

        [Inject]
        public void Construct(IDamageable health)
        {
            _health = health;
        }

        public bool IsAlive => _health != null && _health.IsAlive;

        public void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null)
            => _health?.TakeDamage(amount, hitPoint, force);
    }
}