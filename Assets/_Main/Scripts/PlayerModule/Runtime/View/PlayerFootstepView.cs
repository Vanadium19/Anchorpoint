using UnityEngine;

namespace PlayerModule
{
    public class PlayerFootstepView : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private Vector2 pitchRange = new(0.95f, 1.05f);

        private int _lastClipIndex = -1;

        public void PlayFootstep()
        {
            if (audioSource == null || footstepClips == null || footstepClips.Length == 0)
                return;

            var clipIndex = GetClipIndex();
            var clip = footstepClips[clipIndex];

            if (clip == null)
                return;

            _lastClipIndex = clipIndex;
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            audioSource.PlayOneShot(clip);
        }

        private int GetClipIndex()
        {
            if (footstepClips.Length == 1)
                return 0;

            var clipIndex = Random.Range(0, footstepClips.Length);

            while (clipIndex == _lastClipIndex)
                clipIndex = Random.Range(0, footstepClips.Length);

            return clipIndex;
        }
    }
}
