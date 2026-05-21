using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace AudioModule
{
    public sealed partial class AudioSystem
    {
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<string, List<Action>> callbacks = new();

        public void SubscribeOnCallback(string callbackId, Action callback)
        {
            if (!this.callbacks.TryGetValue(callbackId, out List<Action> callbacks))
            {
                callbacks = new List<Action>(1);
                this.callbacks.Add(callbackId, callbacks);
            }

            callbacks.Add(callback);
        }

        public void UnsubscribeFromCallback(string callbackId, Action callback)
        {
            if (this.callbacks.TryGetValue(callbackId, out List<Action> callbacks))
                callbacks.Remove(callback);
        }

        internal void InvokeCallback(string callbackId)
        {
            if (!this.callbacks.TryGetValue(callbackId, out List<Action> callbacks))
                return;

            for (int i = 0, count = callbacks.Count; i < count; i++)
                callbacks[i].Invoke();
        }
    }
}