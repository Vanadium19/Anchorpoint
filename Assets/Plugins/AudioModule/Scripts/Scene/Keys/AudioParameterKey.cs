using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [Serializable]
    [InlineProperty]
    public struct AudioParameterKey : ISerializationCallbackReceiver
    {
        [HorizontalGroup]
        [OnValueChanged("OnValidate")]
        [SerializeField]
        private AudioBank bank;

        [HorizontalGroup]
        [LabelText("Param")]
        [ValueDropdown("ValuesDropdown")]
        [OnValueChanged("OnValidate")]
        [SerializeField]
        private string parameterId; //Выдавать список параметров dropdown

        [SerializeField, HideInInspector]
        private string identifier;

        public static implicit operator string(AudioParameterKey it) => it.identifier;

        private void OnValidate()
        {
            if (this.bank != null && this.parameterId != null)
                this.identifier = this.bank.identifier + "." + this.parameterId;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (this.bank != null) this.identifier = this.bank.identifier + "." + this.parameterId;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

#if UNITY_EDITOR
        private IEnumerable<string> ValuesDropdown()
        {
            return this.bank == null ? new List<string>() : this.bank.GetAllParameterIds();
        }
#endif
    }
}