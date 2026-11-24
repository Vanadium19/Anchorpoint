using UnityEngine;
using UnityEngine.Audio;

namespace EnemyModule
{
    /// <summary>
    /// Plays the visual effect and the sound of an explosion at a world position.
    /// </summary>
    /// <remarks>
    /// The effect and the sound are spawned as separate objects, so the owner of the feedback can be destroyed right after the call.
    /// A smaller effect is used for explosions whose radius does not exceed <c>smallEffectMaxRadius</c>.
    /// </remarks>
    public sealed class ExplosionFeedback : MonoBehaviour
    {
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private GameObject smallEffectPrefab;
        [SerializeField] [Min(0f)] private float smallEffectMaxRadius = 2.5f;
        [SerializeField] [Min(0.01f)] private float effectScale = 1f;
        [SerializeField] [Min(0f)] private float effectLifetime = 4f;

        [SerializeField] private SoundCue explosionSound;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] [Min(1f)] private float maxAudioDistance = 60f;

        /// <summary>
        /// Plays the explosion effect and sound for an explosion of <paramref name="radius"/> at <paramref name="position"/>.
        /// </summary>
        public void Play(Vector3 position, float radius)
        {
            PlayEffect(position, radius);
            explosionSound.PlayAt(position, mixerGroup, maxAudioDistance);
        }

        private void PlayEffect(Vector3 position, float radius)
        {
            var prefab = smallEffectPrefab != null && radius <= smallEffectMaxRadius ? smallEffectPrefab : effectPrefab;

            if (prefab == null)
                return;

            var effect = Instantiate(prefab, position, Quaternion.identity);
            effect.transform.localScale *= effectScale;
            Destroy(effect, effectLifetime);
        }
    }
}
