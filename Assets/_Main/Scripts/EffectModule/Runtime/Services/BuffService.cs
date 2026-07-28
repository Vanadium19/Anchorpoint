using System;
using System.Collections.Generic;
using BaseModule;
using ComponentsModule;
using Zenject;

namespace EffectModule
{
    public class BuffService : IBuffService, global::Zenject.IInitializable, global::Zenject.ITickable, IDisposable, IPausable
    {
        private readonly Dictionary<IEntity, List<ActiveBuff>> _activeBuffs = new();
        private readonly IPauseManager _pauseManager;

        private bool _isPaused;

        public BuffService(IPauseManager pauseManager)
        {
            _pauseManager = pauseManager;
        }

        public event Action<ActiveBuff> BuffAdded;
        public event Action<ActiveBuff> BuffRemoved;
        public event Action<ActiveBuff> BuffUpdated;

        public void Initialize() => _pauseManager.Register(this);

        public void Dispose() => _pauseManager.Unregister(this);

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            foreach (var kvp in _activeBuffs)
            {
                for (var i = 0; i < kvp.Value.Count; i++)
                {
                    if (isPaused)
                        kvp.Value[i].Buff.Pause();
                    else
                        kvp.Value[i].Buff.Resume();
                }
            }
        }

        public IReadOnlyList<ActiveBuff> GetActiveBuffs(IEntity target)
        {
            if (target == null || !_activeBuffs.TryGetValue(target, out var buffs))
                return new List<ActiveBuff>();

            return buffs.AsReadOnly();
        }

        public void AddBuff(IEntity target, IBuff buff, BuffDataSo buffData = null)
        {
            if (target == null || buff == null)
                return;

            if (!_activeBuffs.ContainsKey(target))
                _activeBuffs[target] = new List<ActiveBuff>();

            var existingBuff = FindBuff(target, buff.BuffId);

            if (existingBuff != null)
            {
                existingBuff.Buff.ResetDuration();
                BuffUpdated?.Invoke(existingBuff);
                return;
            }

            var activeBuff = new ActiveBuff(buff, buffData);
            buff.Completed += OnBuffCompleted;
            buff.Apply(target);

            if (_isPaused)
                buff.Pause();

            _activeBuffs[target].Add(activeBuff);

            BuffAdded?.Invoke(activeBuff);
        }

        private ActiveBuff FindBuff(IEntity target, string buffId)
        {
            if (!_activeBuffs.TryGetValue(target, out var buffs))
                return null;

            foreach (var activeBuff in buffs)
            {
                if (activeBuff.Buff.BuffId == buffId)
                    return activeBuff;
            }

            return null;
        }

        public void RemoveBuff(IEntity target, ActiveBuff activeBuff)
        {
            if (target == null || activeBuff == null)
                return;

            if (!_activeBuffs.TryGetValue(target, out var buffs))
                return;

            if (buffs.Remove(activeBuff))
            {
                activeBuff.Buff.Completed -= OnBuffCompleted;
                activeBuff.Buff.Cancel();
                BuffRemoved?.Invoke(activeBuff);
            }
        }

        public void RemoveAllBuffs(IEntity target)
        {
            if (target == null || !_activeBuffs.TryGetValue(target, out var buffs))
                return;

            foreach (var activeBuff in buffs.ToArray())
            {
                activeBuff.Buff.Completed -= OnBuffCompleted;
                activeBuff.Buff.Cancel();
                BuffRemoved?.Invoke(activeBuff);
            }

            buffs.Clear();
        }

        public bool HasBuff(IEntity target, string buffId)
        {
            if (target == null || string.IsNullOrEmpty(buffId))
                return false;

            if (!_activeBuffs.TryGetValue(target, out var buffs))
                return false;

            foreach (var activeBuff in buffs)
            {
                if (activeBuff.Buff.BuffId == buffId)
                    return true;
            }

            return false;
        }

        public void Tick()
        {
            if (_isPaused)
                return;

            foreach (var kvp in _activeBuffs)
            {
                var buffs = kvp.Value;
                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    var activeBuff = buffs[i];
                    activeBuff.Buff.Tick(UnityEngine.Time.deltaTime);

                    if (!activeBuff.Buff.IsExpired)
                        BuffUpdated?.Invoke(activeBuff);
                }
            }
        }

        private void OnBuffCompleted(IBuff buff)
        {
            buff.Completed -= OnBuffCompleted;

            foreach (var kvp in _activeBuffs)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (kvp.Value[i].Buff == buff)
                    {
                        var activeBuff = kvp.Value[i];
                        kvp.Value.RemoveAt(i);
                        BuffRemoved?.Invoke(activeBuff);
                        return;
                    }
                }
            }
        }
    }
}
