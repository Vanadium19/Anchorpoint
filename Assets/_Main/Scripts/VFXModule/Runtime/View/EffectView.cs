using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VFXModule
{
    public class EffectView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleSystem;

        private EffectId _id;

        public event Action<EffectView> Finished;

        public EffectId Id => _id;

        private void OnValidate() => particleSystem ??= GetComponent<ParticleSystem>();

        public void Initialize(EffectId id) => _id = id;

        public void Play()
        {
            if (particleSystem == null)
            {
                Finished?.Invoke(this);
                return;
            }

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play(true);
            FinishAsync().Forget();
        }

        private async UniTaskVoid FinishAsync()
        {
            await UniTask.Yield();
            await UniTask.WaitWhile(() => particleSystem != null && particleSystem.IsAlive(true));
            Finished?.Invoke(this);
        }
    }
}
