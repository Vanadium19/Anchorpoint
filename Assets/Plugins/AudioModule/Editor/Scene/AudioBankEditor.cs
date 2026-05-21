#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace AudioModule
{
    [CustomEditor(typeof(AudioBank))]
    public sealed class AudioBankEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            this.DrawCompileButton();
            GUILayout.Space(8);
            base.OnInspectorGUI();
        }

        private void DrawCompileButton()
        {
            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 0.83f, 0f, 1);
            if (GUILayout.Button("Compile"))
            {
                this.CompileBank();
            }

            GUI.color = prevColor;
        }

        private void CompileBank()
        {
            AudioBank audioBank = this.target as AudioBank;
            
            AudioBankAPIGenerator.Generate(audioBank);
            
            AssetDatabase.Refresh();

        }
    }
}
#endif