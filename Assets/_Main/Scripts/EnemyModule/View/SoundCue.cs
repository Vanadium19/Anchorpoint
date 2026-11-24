using System;
using UnityEngine;
using UnityEngine.Audio;

namespace EnemyModule
{
    /// <summary>
    /// A set of interchangeable clips with a volume and a pitch range; a random clip is played each time.
    /// </summary>
    [Serializable]
    public sealed class SoundCue
    {
        private const float CleanupPadding = 0.1f;
        private const float MinPitch = 0.1f;

        [SerializeField] private AudioClip[] clips;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private Vector2 pitchRange = new(0.95f, 1.05f);

        /// <summary>
        /// Whether the cue has at least one clip to play.
        /// </summary>
        public bool HasClips => clips != null && clips.Length > 0;

        /// <summary>
        /// Plays a random clip once on <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to play on. Its pitch is overwritten.</param>
        /// <param name="pitchMultiplier">Multiplies the randomly chosen pitch.</param>
        public void Play(AudioSource source, float pitchMultiplier = 1f)
        {
            if (source == null || !HasClips)
                return;

            source.pitch = PickPitch() * pitchMultiplier;
            source.PlayOneShot(PickClip(), volume);
        }

        /// <summary>
        /// Plays a random clip once from a temporary object at <paramref name="position"/>, which outlives the caller.
        /// </summary>
        public void PlayAt(Vector3 position, AudioMixerGroup mixerGroup, float maxDistance)
        {
            if (!HasClips)
                return;

            var clip = PickClip();
            var pitch = Mathf.Max(PickPitch(), MinPitch);

            var audioObject = new GameObject("OneShotAudio");
            audioObject.transform.position = position;

            var source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = mixerGroup;
            source.volume = volume;
            source.pitch = pitch;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.maxDistance = maxDistance;
            source.Play();

            UnityEngine.Object.Destroy(audioObject, clip.length / pitch + CleanupPadding);
        }

        private AudioClip PickClip() => clips[UnityEngine.Random.Range(0, clips.Length)];

        private float PickPitch() => UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
    }
}
