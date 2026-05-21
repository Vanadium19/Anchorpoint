#if UNITY_EDITOR
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace AudioModule
{
    [UsedImplicitly]
    public sealed class UISoundKeyDrawer : OdinAttributeDrawer<UISoundKeyAttribute, string>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            UISoundCatalog bank = FindUISoundCatalog();
            
            if (bank == null)
            {
                GUILayout.Label(label);
                EditorGUILayout.HelpBox("No catalog in project", MessageType.Error);
                return;
            }

            if (bank.GetCount() <= 0)
            {
                GUILayout.Label(label);
                EditorGUILayout.HelpBox("Catalog is empty", MessageType.Error);
                return;
            }

            string soundId = this.ValueEntry.SmartValue;

            int index = bank.IndexOfSound(soundId);
            if (index == -1 || string.IsNullOrEmpty(soundId))
            {
                index = 0;
            }
            
            GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth);
            index = EditorGUILayout.Popup(label, index, bank.GetKeys());
            this.ValueEntry.SmartValue = bank.GetSoundAt(index).Item1;

            GUIHelper.PopLabelWidth();
        }
        
        private static UISoundCatalog FindUISoundCatalog()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(UISoundCatalog));
            
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                UISoundCatalog catalog = AssetDatabase.LoadAssetAtPath<UISoundCatalog>(assetPath);
                
                if (catalog.isMain)
                {
                    return catalog;
                }
            }

            return null;
        }
    }
}
#endif