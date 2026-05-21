using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace AudioModule
{
    public partial class AudioSystem
    {
        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<string, bool> boolParameters = new();

        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<string, int> intParameters = new();

        [FoldoutGroup("Parameters")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<string, float> floatParameters = new();

        public bool TryGetBool(string paramId, out bool result) => this.boolParameters.TryGetValue(paramId, out result);
        public bool TryGetInt(string paramId, out int result) => this.intParameters.TryGetValue(paramId, out result);
        public bool TryGetFloat(string paramId, out float result) => this.floatParameters.TryGetValue(paramId, out result);

        public bool GetBool(string paramId) => this.boolParameters[paramId];
        public int GetInt(string paramId) => this.intParameters[paramId];
        public float GetFloat(string paramId) => this.floatParameters[paramId];

        public bool HasFloat(string paramId) => this.floatParameters.ContainsKey(paramId);
        public bool HasInt(string paramId) => this.intParameters.ContainsKey(paramId);
        public bool HasBool(string paramId) => this.boolParameters.ContainsKey(paramId);

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public void SetBool(string paramId, bool value) => this.boolParameters[paramId] = value;

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public void SetInt(string paramId, int value) => this.intParameters[paramId] = value;

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public void SetFloat(string paramId, float value) => this.floatParameters[paramId] = value;

    }
}