using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace AudioModule
{
    public sealed partial class AudioSystem
    {
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<string, Action> triggers = new();
        
        public void SetTrigger(string triggerId, Action trigger) => this.triggers[triggerId] = trigger;

        public void ResetTrigger(string triggerId) => this.triggers.Remove(triggerId);
        
        internal void InvokeTrigger(string triggerId)
        {
            if (this.triggers.Remove(triggerId, out Action trigger)) 
                trigger.Invoke();
        }
    }
}