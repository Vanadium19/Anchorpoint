using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable, InlineProperty]
    public struct AudioEventKey : ISerializationCallbackReceiver
    {
        [HorizontalGroup]
        [OnValueChanged("OnValidate")]
        [SerializeField]
        private AudioBank bank;

        [HorizontalGroup]
        [ValueDropdown("ValuesDropdown")]
        [LabelText("Event")]
        [OnValueChanged("OnValidate")]
        [SerializeField]
        private string eventId; //Выдавать список ивентов dropdown

        [SerializeField, HideInInspector]
        private string identifier;

        public static implicit operator string(AudioEventKey it) => it.identifier;

        private void OnValidate()
        {
            if (this.bank != null && this.eventId != null) 
                this.identifier = this.bank.identifier + "." + this.eventId;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.OnValidate();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

#if UNITY_EDITOR
        private IEnumerable<string> ValuesDropdown()
        {
            return this.bank == null ? new List<string>() : this.bank.GetEventIds();
        }
#endif
    }
}