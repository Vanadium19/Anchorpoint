using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Renders a <see cref="RandomEventPool"/> as a picker type-picker plus a reorderable list of weighted events.</summary>
    [CustomPropertyDrawer(typeof(RandomEventPool))]
    public class RandomEventPoolDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> _lists = new();

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pickerProperty = ManagedReferenceEditorGUI.RequireRelative(property, "picker");
            var list = GetOrCreateList(property);

            var pickerHeight = ManagedReferenceEditorGUI.GetReferenceFieldHeight(pickerProperty);
            var pickerRect = new Rect(position.x, position.y, position.width, pickerHeight);
            ManagedReferenceEditorGUI.DrawReferenceField(pickerRect, pickerProperty, typeof(IRandomEventPickerAsset), "Picker");

            var listRect = new Rect(position.x, pickerRect.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, list.GetHeight());
            list.DoList(listRect);
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var pickerProperty = ManagedReferenceEditorGUI.RequireRelative(property, "picker");

            return ManagedReferenceEditorGUI.GetReferenceFieldHeight(pickerProperty)
                + EditorGUIUtility.standardVerticalSpacing
                + GetOrCreateList(property).GetHeight();
        }

        private ReorderableList GetOrCreateList(SerializedProperty property)
        {
            var entriesProperty = ManagedReferenceEditorGUI.RequireRelative(property, "entries");

            if (_lists.TryGetValue(property.propertyPath, out var cachedList))
            {
                cachedList.serializedProperty = entriesProperty;
                return cachedList;
            }

            var list = new ReorderableList(entriesProperty.serializedObject, entriesProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Events", EditorStyles.boldLabel),
                elementHeightCallback = index =>
                    EditorGUI.GetPropertyHeight(entriesProperty.GetArrayElementAtIndex(index), true) + RandomEventEditorStyles.ListElementSpacing,
                drawElementCallback = (rect, index, _, _) =>
                {
                    var elementRect = new Rect(rect.x, rect.y + RandomEventEditorStyles.ListElementSpacing * 0.5f, rect.width, rect.height - RandomEventEditorStyles.ListElementSpacing);
                    EditorGUI.PropertyField(elementRect, entriesProperty.GetArrayElementAtIndex(index), GUIContent.none, true);
                },
            };

            _lists[property.propertyPath] = list;

            return list;
        }
    }
}
