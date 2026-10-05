using UnityEngine;

namespace BuildingModule
{
    public sealed class DefenseBuildingView : MonoBehaviour
    {
        [SerializeField] private Transform rotationPivot;
        [SerializeField] private Transform projectileNode;
        [SerializeField] private Animator animator;
        [SerializeField] private DefenseProjectileView projectilePrefab;
        [SerializeField] private string attackStateName = "turret_attack_anim";
        [SerializeField] [Min(0.1f)] private float projectileSpeed = 25f;
        [SerializeField] private float rotationOffsetY;

        public void Attack(Vector3 targetPosition)
        {
            AimAt(targetPosition);
            PlayAttackAnimation();
            SpawnProjectile(targetPosition);
        }

        private void AimAt(Vector3 targetPosition)
        {
            if (rotationPivot == null)
                return;

            var direction = targetPosition - rotationPivot.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
                return;

            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            rotationPivot.rotation = targetRotation * Quaternion.Euler(0f, rotationOffsetY, 0f);
        }

        private void PlayAttackAnimation()
        {
            if (animator == null || string.IsNullOrWhiteSpace(attackStateName))
                return;

            animator.Play(attackStateName, 0, 0f);
        }

        private void SpawnProjectile(Vector3 targetPosition)
        {
            if (projectileNode == null || projectilePrefab == null)
                return;

            var projectile = Instantiate(
                projectilePrefab,
                projectileNode.position,
                projectileNode.rotation);

            projectile.Initialize(targetPosition, projectileSpeed);
        }
    }
}
