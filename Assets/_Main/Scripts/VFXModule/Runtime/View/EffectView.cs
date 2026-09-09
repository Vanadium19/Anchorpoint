using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VFXModule
{
    public class EffectView : MonoBehaviour, IEffectHandle
    {
        [SerializeField] private ParticleSystem particleSystem;

        private EffectId _id;
        private int _playVersion;
        private bool _isPlaying;

        public event Action<EffectView> Finished;

        public EffectId Id => _id;

        private void OnValidate() => particleSystem ??= GetComponent<ParticleSystem>();

        public void Initialize(EffectId id) => _id = id;

        public void SetScale(Vector3 effectScale)
        {
            if (particleSystem == null)
                return;

            particleSystem.transform.localScale = effectScale;
        }

        public void Play()
        {
            _playVersion++;
            _isPlaying = true;

            if (particleSystem == null)
            {
                Finish();
                return;
            }

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play(true);
            FinishAsync(_playVersion).Forget();
        }

        public void Stop()
        {
            if (!_isPlaying)
                return;

            if (particleSystem == null)
            {
                Finish();
                return;
            }

            // Stop spawning but let live particles fade out; FinishAsync returns the effect to the pool once none remain.
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private async UniTaskVoid FinishAsync(int playVersion)
        {
            await UniTask.Yield();

            await UniTask.WaitWhile(() =>
                this != null &&
                playVersion == _playVersion &&
                particleSystem != null &&
                particleSystem.IsAlive(true));

            if (this == null || playVersion != _playVersion || !_isPlaying)
                return;

            Finish();
        }

        private void Finish()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;
            Finished?.Invoke(this);
        }
    }
}
