using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioModule
{
    [CreateAssetMenu(fileName = "AudioBank", menuName = "Game/Audio/Scene/AudioBank")]
    public sealed class AudioBank : ScriptableObject
    {
        [SerializeField]
        internal string identifier;
        
        [SerializeField]
        internal string codeGenPath = "Assets/_Main/Scripts/AudioModule/Codegen";

        [Space(8)]
        [SerializeField]
        internal EventParameter[] eventParameters;

        [Space(8)]
        [SerializeField]
        internal BoolParameter[] boolParameters;

        [Space(8)]
        [SerializeField]
        internal IntParameter[] intParameters;

        [Space(8)]
        [SerializeField]
        internal FloatParameter[] floatParameters;

        [Space(8)]
        [SerializeField]
        internal string[] callbacks;

        public string GetCodeGenPath() => this.codeGenPath;

        public IReadOnlyList<string> GetCallbacks() => this.callbacks;

        public string GetIdentifier() => this.identifier;

        public IEnumerable<string> GetEventIds() => this.eventParameters.Select(it => it.identifier);

        public IEnumerable<string> GetAllParameterIds()
        {
            foreach (BoolParameter parameter in this.boolParameters)
                yield return parameter.identifier;

            foreach (IntParameter parameter in this.intParameters)
                yield return parameter.identifier;

            foreach (FloatParameter parameter in this.floatParameters)
                yield return parameter.identifier;
        }
    }
}