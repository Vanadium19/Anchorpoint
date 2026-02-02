using UnityEngine;

namespace InventoryModule
{
    public class LootItemView : MonoBehaviour
    {
        [SerializeField] private ItemDefinition itemDefinition;
        [SerializeField] private int amount = 1;

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private ParticleSystem pickupParticles;
        [SerializeField] private AudioClip pickupSound;
        private ItemDefinition _cachedDefinition;

        public ItemDefinition ItemDef => GetItemDefinition();
        public int Amount => amount;

        public void SetAmount(int newAmount)
        {
            amount = Mathf.Max(1, newAmount);
        }

        private ItemDefinition GetItemDefinition()
        {
            if (_cachedDefinition != null)
                return _cachedDefinition;

            if (itemDefinition != null)
            {
                _cachedDefinition = itemDefinition;
                return _cachedDefinition;
            }

            return null;
        }
        public void Initialize(ItemDefinition definition, int newAmount)
        {
            this.itemDefinition = definition;
            this.amount = newAmount;
            _cachedDefinition = definition;
        }
        public void Collect()
        {
            if (pickupParticles != null)
            {
                var particles = Instantiate(pickupParticles, transform.position, Quaternion.identity);
                Destroy(particles.gameObject, 2f);
            }

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}