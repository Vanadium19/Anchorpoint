using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    internal sealed class AudioEventPool : IDisposable
    {
        private readonly AudioSystem _audioSystem;

        [ShowInInspector, ReadOnly]
        private readonly Dictionary<string, (AudioEventBase, Stack<AudioEventBase>)> _poolMap = new();

        internal AudioEventPool(AudioSystem audioSystem)
        {
            _audioSystem = audioSystem;
        }

        internal void RegisterPrefab(string identifier, AudioEventBase prefab)
        {
            Stack<AudioEventBase> stack = prefab.Poolable ? new Stack<AudioEventBase>() : null; 
            _poolMap[identifier] = (prefab, stack);
        }

        internal void UnregisterPrefab(string identifier)
        {
            if (!_poolMap.Remove(identifier, out (AudioEventBase prefab, Stack<AudioEventBase> stack) tuple))
                return;

            if (!tuple.prefab.Poolable)
                return;

            foreach (var audioEvent in tuple.stack)
                ScriptableObject.Destroy(audioEvent);

            tuple.stack.Clear();
        }

        internal AudioEventBase Get(string identifier)
        {
            if (!_poolMap.TryGetValue(identifier, out (AudioEventBase prefab, Stack<AudioEventBase> stack) tuple))
                throw new Exception($"Event id {identifier} is not found!");
            
            if (!tuple.prefab.Poolable || !tuple.stack.TryPop(out AudioEventBase evt))
            {
                evt = ScriptableObject.Instantiate(tuple.prefab);
                evt.Initialize(identifier, _audioSystem);
            }

            evt.OnSpawn();
            return evt;
        }

        internal bool TryGet(string identifier, out AudioEventBase evt)
        {
            if (!_poolMap.TryGetValue(identifier, out (AudioEventBase prefab, Stack<AudioEventBase> stack) tuple))
            {
                evt = default;
                return false;
            }

            if (!tuple.prefab.Poolable || !tuple.stack.TryPop(out evt))
            {
                evt = ScriptableObject.Instantiate(tuple.prefab);
                evt.Initialize(identifier, _audioSystem);
            }

            evt.OnSpawn();
            return true;
        }

        internal void Release(AudioEventBase audioEvent)
        {
            string identifier = audioEvent.Id;
            audioEvent.OnDespawn();

            if (_poolMap.TryGetValue(identifier, out (AudioEventBase prefab, Stack<AudioEventBase> stack) tuple))
            {
                if (tuple.prefab.Poolable) 
                    tuple.stack.Push(audioEvent);
                else
                    ScriptableObject.Destroy(audioEvent);
            }
        }

        public void Dispose()
        {
            foreach ((AudioEventBase prefab, Stack<AudioEventBase> stack) in _poolMap.Values)
            {
                if (!prefab.Poolable)
                    continue;

                foreach (AudioEventBase audioEvent in stack) 
                    ScriptableObject.Destroy(audioEvent);

                stack.Clear();
            }

            _poolMap.Clear();
        }
    }
}