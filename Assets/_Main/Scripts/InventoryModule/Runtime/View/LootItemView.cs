using UnityEngine;

namespace InventoryModule
{
    public sealed class LootItemView : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _itemDefinition;
        [SerializeField] private int _amount;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private ParticleSystem _pickupParticles;
        [SerializeField] private AudioClip _pickupSound;

        private ItemDefinition _cachedDefinition;

        public ItemDefinition ItemDef => GetItemDefinition();
        public int Amount => _amount;

        public void Initialize(ItemDefinition definition, int newAmount)
        {
            _itemDefinition = definition;
            _amount = newAmount;
            _cachedDefinition = definition;
        }

        public void SetAmount(int newAmount)
        {
            _amount = Mathf.Max(1, newAmount);
        }

        public void PlayCollectEffects()
        {
            if (_pickupParticles != null)
            {
                var particles = Instantiate(_pickupParticles, transform.position, Quaternion.identity);
                Destroy(particles.gameObject, 2f);
            }

            if (_pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position);
            }
        }

        private ItemDefinition GetItemDefinition()
        {
            if (_cachedDefinition != null)
                return _cachedDefinition;

            if (_itemDefinition != null)
            {
                _cachedDefinition = _itemDefinition;
                return _cachedDefinition;
            }

            return null;
        }
    }
}
