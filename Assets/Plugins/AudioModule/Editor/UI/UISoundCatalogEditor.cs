using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace AudioModule
{
    [CustomEditor(typeof(UISoundCatalog))]
    public sealed class UISoundCatalogEditor : OdinEditor
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
                this.CompileSounds();
            }

            GUI.color = prevColor;
        }

        private void CompileSounds()
        {
            UISoundCatalog uiSoundCatalog = this.target as UISoundCatalog;
            IReadOnlyList<UISoundCatalog.SoundInfo> sounds = uiSoundCatalog!.sounds;
            
            UISoundAPIGenerator.Generate(sounds);
            UISoundComponentGenerator.Generate(sounds);
            
            AssetDatabase.Refresh();

        }
    }
}